using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.BackgroundJobs.Transaction;
using EMRS.Application.Abstractions.Models.VNPay;
using EMRS.Application.Common;
using EMRS.Application.DTOs.WalletDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class WalletService : IWalletService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVNPayService _vnPayService;
    private readonly ITransactionJobScheduler _transactionJobScheduler;

    public WalletService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IVNPayService vnPayService,
        ITransactionJobScheduler transactionJobScheduler)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _vnPayService = vnPayService;
        _transactionJobScheduler = transactionJobScheduler;
    }
    public async Task<bool> TransferMoneyAsync(Wallet fromWallet, Wallet toWallet, decimal amount)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {

            if (fromWallet == null || toWallet == null)
                return false;

            if (fromWallet.Balance < amount)
                return false;

            fromWallet.Balance -= amount;
            toWallet.Balance += amount;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            return false;
        }
    }

    public async Task<ResultResponse<WalletResponse>> CreateWalletAsync()
    {
        try
        {
            var renterId = Guid.Parse(_currentUserService.UserId);

            var renter = await _unitOfWork.GetRenterRepository()
                .Query()
                .Where(r => r.Id == renterId)
                .FirstOrDefaultAsync();

            if (renter == null)
            {
                return ResultResponse<WalletResponse>.NotFound("Renter not found for current user");
            }

            var existingWallet = await _unitOfWork.GetWalletRepository()
                .GetWalletByRenterIdAsync(renter.Id);

            if (existingWallet != null)
            {
                return ResultResponse<WalletResponse>.Failure("Renter already has a wallet");
            }

            var newWallet = new Wallet
            {
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                RenterId = renter.Id
            };
            await _unitOfWork.GetWalletRepository().AddAsync(newWallet);
            await _unitOfWork.SaveChangesAsync();
            var walletResponse = new WalletResponse
            {
                Id = newWallet.Id,
                Balance = newWallet.Balance,
                RenterId = renter.Id
            };
            return ResultResponse<WalletResponse>.SuccessResult("Wallet created successfully.", walletResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<WalletResponse>.Failure($"Error creating wallet: {ex.Message}");
        }
    }

    public async Task<ResultResponse<WalletBalanceResponse>> GetMyWalletBalanceAsync()
    {
        try
        {
            var userId = Guid.Parse(_currentUserService.UserId);

            var wallet = await _unitOfWork.GetWalletRepository()
                .GetWalletByRenterIdAsync(userId);

            if (wallet == null)
            {
                return ResultResponse<WalletBalanceResponse>.NotFound(
                    "Wallet not found for this user");
            }

            if (wallet.RenterId == null)
            {
                return ResultResponse<WalletBalanceResponse>.Failure(
                    "Wallet is not associated with any renter");
            }
            var response = new WalletBalanceResponse
            {
                Balance = wallet.Balance,
                RenterId = wallet.RenterId.Value
            };

            return ResultResponse<WalletBalanceResponse>.SuccessResult(
                "Wallet balance retrieved successfully",
                response);
        }
        catch (Exception ex)
        {
            return ResultResponse<WalletBalanceResponse>.Failure(
                $"Error retrieving wallet balance: {ex.Message}");
        }
    }

    public async Task<ResultResponse<WalletTopUpResponse>> CreateTopUpRequestAsync(WalletTopUpRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            
            //if (request.Amount % 10000 != 0)
            //{
            //    return ResultResponse<WalletTopUpResponse>.Failure(
            //        "Amount must be a multiple of 10,000 VND");
            //}

            
            var userId = Guid.Parse(_currentUserService.UserId);

            
            var wallet = await _unitOfWork.GetWalletRepository()
                .GetWalletByRenterIdForModifyAsync(userId);

            if (wallet == null)
            {
                return ResultResponse<WalletTopUpResponse>.Failure(
                    "Wallet not found for this user");
            }

            
            var transactionCode = Generator.TransactionCodeGenerate();
            var newTransaction = new Transaction
            {
                TransactionType = TransactionTypeEnum.WalletTopUp.ToString(),
                Amount = request.Amount,
                DocNo = wallet.Id, 
                Status = TransactionStatusEnum.Pending.ToString()
            };

            await _unitOfWork.GetTransactionRepository().AddAsync(newTransaction);
            await _unitOfWork.SaveChangesAsync();

            
            var vnpayRequest = new VNPayRequestData
            {
                Amount = request.Amount,
                OrderDescription = $"Nap tien vao vi - Transaction: {newTransaction.Id}",
                OrderId = newTransaction.Id.ToString() // Dùng TransactionId làm OrderId
            };

            string vnpayUrl = _vnPayService.CreatePaymentUrlWallet(vnpayRequest);

            await _unitOfWork.CommitAsync();

            
            var response = new WalletTopUpResponse
            {
                TransactionId = newTransaction.Id,
                Amount = request.Amount,
                TransactionCode = transactionCode,
                Status = newTransaction.Status,
                VNPayUrl = vnpayUrl,
                CreatedAt = newTransaction.CreatedAt
            };

            //Schedule auto-cancel sau 15 phút
            _transactionJobScheduler.ScheduleAutoCancel(newTransaction.Id, TimeSpan.FromMinutes(15));

            return ResultResponse<WalletTopUpResponse>.SuccessResult(
                "Top-up request created successfully. Please complete payment within 15 minutes.",
                response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<WalletTopUpResponse>.Failure(
                $"An error occurred while creating top-up request: {ex.Message}");
        }
    }

    /// <summary>
    /// Xử lý callback từ VNPay sau khi thanh toán
    /// </summary>
    public async Task<ResultResponse<bool>> ProcessTopUpCallbackAsync(VNPayResponseData vnPayResponse)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            
            if (!vnPayResponse.IsSuccess)
            {
                return ResultResponse<bool>.Failure(vnPayResponse.Message);
            }

            
            if (!Guid.TryParse(vnPayResponse.OrderId, out var transactionId))
            {
                return ResultResponse<bool>.Failure("Invalid transaction ID");
            }

            var transaction = await _unitOfWork.GetTransactionRepository()
                .FindByIdAsync(transactionId);

            if (transaction == null)
            {
                return ResultResponse<bool>.NotFound("Transaction not found");
            }

            
            if (transaction.Status == TransactionStatusEnum.Success.ToString())
            {
                return ResultResponse<bool>.SuccessResult(
                    "Transaction already processed",
                    true);
            }

            
            var wallet = await _unitOfWork.GetWalletRepository()
                .FindByIdAsync(transaction.DocNo);

            if (wallet == null)
            {
                return ResultResponse<bool>.NotFound("Wallet not found");
            }

            
            if (vnPayResponse.ResponseCode == "00")
            {
                
                wallet.Balance += transaction.Amount;
                transaction.Status = TransactionStatusEnum.Success.ToString();

                _unitOfWork.GetWalletRepository().Update(wallet);
                _unitOfWork.GetTransactionRepository().Update(transaction);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return ResultResponse<bool>.SuccessResult(
                    $"Top-up successful. {transaction.Amount:N0} VND added to wallet.",
                    true);
            }
            else
            {
                
                transaction.Status = TransactionStatusEnum.Failed.ToString();
                _unitOfWork.GetTransactionRepository().Update(transaction);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return ResultResponse<bool>.Failure(
                    $"Payment failed: {vnPayResponse.Message} (Code: {vnPayResponse.ResponseCode})");
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<bool>.Failure(
                $"VNPay callback error: {ex.Message}");
        }
    }

    // Auto-cancel transaction nếu không thanh toán trong 15 phút

    public async Task<ResultResponse<bool>> AutoCancelTopUpRequestAsync(Guid transactionId)
    {
        try
        {
            var transaction = await _unitOfWork.GetTransactionRepository()
                .FindByIdAsync(transactionId);

            if (transaction == null)
            {
                return ResultResponse<bool>.NotFound("Transaction not found");
            }

            
            if (transaction.Status == TransactionStatusEnum.Pending.ToString())
            {
                transaction.Status = TransactionStatusEnum.Failed.ToString();
                _unitOfWork.GetTransactionRepository().Update(transaction);
                await _unitOfWork.SaveChangesAsync();

                return ResultResponse<bool>.SuccessResult(
                    "Top-up request auto-cancelled due to timeout",
                    true);
            }

            return ResultResponse<bool>.SuccessResult(
                "Transaction already processed",
                true);
        }
        catch (Exception ex)
        {
            return ResultResponse<bool>.Failure(
                $"Error auto-cancelling transaction: {ex.Message}");
        }
    }

}
