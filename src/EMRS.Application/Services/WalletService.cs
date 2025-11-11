using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.WalletDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class WalletService:IWalletService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    public WalletService(IUnitOfWork unitOfWork,IMapper mapper, ICurrentUserService currentUserService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
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
            var newWallet = new Wallet
            {
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.GetWalletRepository().AddAsync(newWallet);
            await _unitOfWork.SaveChangesAsync();
            WalletResponse walletResponse=_mapper.Map<WalletResponse>(newWallet);
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
}
