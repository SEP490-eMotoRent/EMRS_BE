using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.TransactionDTOs;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultResponse<List<TransactionResponse>>> GetAllAsync()
        {
            try
            {
                var transactions = await _unitOfWork.GetTransactionRepository().GetAllAsync();

                var response = transactions.Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    DocNo = t.DocNo,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                }).ToList();

                return ResultResponse<List<TransactionResponse>>.SuccessResult("Transaction Retrived",response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<TransactionResponse>>.Failure($"Error fetching transactions: {ex.Message}");
            }
        }

        public async Task<ResultResponse<TransactionResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _unitOfWork.GetTransactionRepository().FindByIdAsync(id);

                if (entity == null)
                    return ResultResponse<TransactionResponse>.Failure("Transaction not found.");

                var response = new TransactionResponse
                {
                    Id = entity.Id,
                    TransactionType = entity.TransactionType,
                    Amount = entity.Amount,
                    DocNo = entity.DocNo,
                    Status = entity.Status,
                    CreatedAt = entity.CreatedAt
                };

                return ResultResponse<TransactionResponse>.SuccessResult("Transaction Retrived", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<TransactionResponse>.Failure($"Error getting transaction: {ex.Message}");
            }
        }

        public async Task<ResultResponse<TransactionResponse>> CreateAsync(TransactionCreateRequest request)
        {
            try
            {
                var entity = new Transaction
                {
                    TransactionType = request.TransactionType,
                    Amount = request.Amount,
                    DocNo = request.DocNo,
                    Status = request.Status
                };

                await _unitOfWork.GetTransactionRepository().AddAsync(entity);

                var response = new TransactionResponse
                {
                    Id = entity.Id,
                    TransactionType = entity.TransactionType,
                    Amount = entity.Amount,
                    DocNo = entity.DocNo,
                    Status = entity.Status,
                    CreatedAt = entity.CreatedAt
                };

                return ResultResponse<TransactionResponse>.SuccessResult("Transaction Retrived", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<TransactionResponse>.Failure($"Error creating transaction: {ex.Message}");
            }
        }

        public async Task<ResultResponse<TransactionResponse>> UpdateAsync(Guid id, TransactionUpdateRequest request)
        {
            try
            {
                var entity = await _unitOfWork.GetTransactionRepository().FindByIdAsync(id);

                if (entity == null)
                    return ResultResponse<TransactionResponse>.Failure("Transaction not found.");

                entity.TransactionType = request.TransactionType;
                entity.Amount = request.Amount;
                entity.DocNo = request.DocNo;
                entity.Status = request.Status;

                _unitOfWork.GetTransactionRepository().Update(entity);

                var response = new TransactionResponse
                {
                    Id = entity.Id,
                    TransactionType = entity.TransactionType,
                    Amount = entity.Amount,
                    DocNo = entity.DocNo,
                    Status = entity.Status,
                    CreatedAt = entity.CreatedAt
                };

                return ResultResponse<TransactionResponse>.SuccessResult("Transaction Retrived", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<TransactionResponse>.Failure($"Error updating transaction: {ex.Message}");
            }
        }

        public async Task<ResultResponse<TransactionResponse>> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _unitOfWork.GetTransactionRepository().FindByIdAsync(id);
                var response= new TransactionResponse
                {
                    Id = entity.Id,
                    TransactionType = entity.TransactionType,
                    Amount = entity.Amount,
                    DocNo = entity.DocNo,
                    Status = entity.Status,
                    CreatedAt = entity.CreatedAt
                };
                if (entity == null)
                    return ResultResponse<TransactionResponse>.Failure("Transaction not found.");

                _unitOfWork.GetTransactionRepository().Delete(entity);

                return ResultResponse<TransactionResponse>.SuccessResult("Transaction deleted successfully.", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<TransactionResponse>.Failure($"Error deleting transaction: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<TransactionResponse>>> GetByRenterIdAsync(Guid renterId)
        {
            try
            {
                // Sử dụng phương pháp query gián tiếp
                var transactions = await _unitOfWork.GetTransactionRepository()
                    .GetTransactionsByRenterIdAsync(renterId);

                if (!transactions.Any())
                {
                    return ResultResponse<List<TransactionResponse>>.NotFound(
                        "No transactions found for this renter");
                }

                var response = transactions.Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    DocNo = t.DocNo,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                }).ToList();

                return ResultResponse<List<TransactionResponse>>.SuccessResult(
                    "Transactions retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<TransactionResponse>>.Failure(
                    $"Error fetching transactions: {ex.Message}");
            }
        }

        public async Task<ResultResponse<TransactionTotalResponse>> GetTotalRevenueAsync()
        {
            try
            {
                var transactions = await _unitOfWork.GetTransactionRepository().GetAllAsync();

                var successTransactions = transactions
                    .Where(t => t.Status == TransactionStatusEnum.Success.ToString());

                var groupedSums = successTransactions
                    .GroupBy(t => t.TransactionType)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                decimal totalRevenue = 0;
                totalRevenue += groupedSums.GetValueOrDefault(TransactionTypeEnum.BookingDeposit.ToString(), 0);
                totalRevenue += groupedSums.GetValueOrDefault(TransactionTypeEnum.BookingFinalPayment.ToString(), 0);
                totalRevenue += groupedSums.GetValueOrDefault(TransactionTypeEnum.BookingAdditionalPayment.ToString(), 0);
                totalRevenue += groupedSums.GetValueOrDefault(TransactionTypeEnum.Penalty.ToString(), 0);
                totalRevenue -= groupedSums.GetValueOrDefault(TransactionTypeEnum.BookingRefund.ToString(), 0);
                totalRevenue -= groupedSums.GetValueOrDefault(TransactionTypeEnum.InsuranceClaimRefund.ToString(), 0);
                totalRevenue -= groupedSums.GetValueOrDefault(TransactionTypeEnum.Refund.ToString(), 0);

                var response = new TransactionTotalResponse
                {
                    TotalRevenue = totalRevenue
                };

                return ResultResponse<TransactionTotalResponse>.SuccessResult(
                    "Total revenue calculated successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<TransactionTotalResponse>.ServerError(
                    $"Error calculating total revenue: {ex.Message}");
            }
        }

    }
}
