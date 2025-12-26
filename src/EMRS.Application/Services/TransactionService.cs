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
                totalRevenue += groupedSums.GetValueOrDefault(TransactionTypeEnum.BookingAdditionalPayment.ToString(), 0);
                totalRevenue -= groupedSums.GetValueOrDefault(TransactionTypeEnum.BookingRefund.ToString(), 0);
                totalRevenue -= groupedSums.GetValueOrDefault(TransactionTypeEnum.InsuranceClaimRefund.ToString(), 0);
                totalRevenue -= groupedSums.GetValueOrDefault(TransactionTypeEnum.BookingReturnRefund.ToString(), 0);

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
        public async Task<ResultResponse<TransactionTotalResponseMonths>> GetTotalRevevenueMonths(int year)
        {
            try
            {
                var transactions =
                    await _unitOfWork
                        .GetTransactionRepository()
                        .GetTransactionsByVietnamYearAsync(year);

                var successTransactions = transactions
                    .Where(t => t.Status == TransactionStatusEnum.Success.ToString())
                    .ToList();

                var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                var monthTotals = successTransactions
                    .GroupBy(t =>
                    {
                        var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
                            t.CreatedAt.UtcDateTime,
                            vnTz
                        );
                        return vnTime.Month;
                    })
                    .Select(g =>
                    {
                        var typeSums = g
                            .GroupBy(x => x.TransactionType)
                            .ToDictionary(
                                x => x.Key,
                                x => x.Sum(v => v.Amount)
                            );

                        decimal revenue = 0;
                        revenue += typeSums.GetValueOrDefault(TransactionTypeEnum.BookingDeposit.ToString(), 0);
                        revenue += typeSums.GetValueOrDefault(TransactionTypeEnum.BookingAdditionalPayment.ToString(), 0);
                        revenue -= typeSums.GetValueOrDefault(TransactionTypeEnum.BookingRefund.ToString(), 0);
                        revenue -= typeSums.GetValueOrDefault(TransactionTypeEnum.InsuranceClaimRefund.ToString(), 0);
                        revenue -= typeSums.GetValueOrDefault(TransactionTypeEnum.BookingReturnRefund.ToString(), 0);

                        return new TransactionMonthTotal
                        {
                            Month = g.Key,
                            TotalRevenue = revenue
                        };
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                var fullMonths = Enumerable.Range(1, 12)
                    .Select(month => new TransactionMonthTotal
                    {
                        Month = month,
                        TotalRevenue = monthTotals
                            .FirstOrDefault(x => x.Month == month)?.TotalRevenue ?? 0
                    })
                    .ToList();

                return ResultResponse<TransactionTotalResponseMonths>.SuccessResult(
                    "Get revenue by month successfully",
                    new TransactionTotalResponseMonths
                    {
                        monthTotals = fullMonths
                    });
            }
            catch (Exception ex)
            {
                return ResultResponse<TransactionTotalResponseMonths>.ServerError(
                    $"Error calculating total revenue: {ex.Message}");
            }
        }
        public async Task<ResultResponse<List<TransactionTotalResponsePerDay>>> GetTotalRevevenuePerMonth(int year,int month)
        {
            try
            {
                var transactions =
                    await _unitOfWork
                        .GetTransactionRepository()
                        .GetTransactionsByVietnamMonthAsync(year,month);

                var successTransactions = transactions
                    .Where(t => t.Status == TransactionStatusEnum.Success.ToString())
                    .ToList();
                var daysInMonth = DateTime.DaysInMonth(year, month);

                var allDays = Enumerable
                    .Range(1, daysInMonth)
                    .Select(day => new DateOnly(year, month, day))
                    .ToList();

                var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                var revenueByDate = successTransactions
    .GroupBy(t =>
    {
        var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
            t.CreatedAt.UtcDateTime,
            vnTz
        );
        return DateOnly.FromDateTime(vnTime);
    })
    .ToDictionary(g => g.Key, g =>
    {
        var typeSums = g
            .GroupBy(x => x.TransactionType)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(v => v.Amount)
            );

        var grossRevenue =
              typeSums.GetValueOrDefault(TransactionTypeEnum.BookingDeposit.ToString(), 0)
            + typeSums.GetValueOrDefault(TransactionTypeEnum.BookingAdditionalPayment.ToString(), 0);

        var refundAmount =
              typeSums.GetValueOrDefault(TransactionTypeEnum.BookingRefund.ToString(), 0)
            + typeSums.GetValueOrDefault(TransactionTypeEnum.BookingReturnRefund.ToString(), 0)
            + typeSums.GetValueOrDefault(TransactionTypeEnum.InsuranceClaimRefund.ToString(), 0);

        return new { grossRevenue, refundAmount };
    });
                var result = allDays
                    .Select(date =>
                    {
                        if (revenueByDate.TryGetValue(date, out var revenue))
                        {
                            return new TransactionTotalResponsePerDay
                            {
                                Date = date,
                                GrossRevenue = revenue.grossRevenue,
                                RefundAmount = revenue.refundAmount
                            };
                        }

                        return new TransactionTotalResponsePerDay
                        {
                            Date = date,
                            GrossRevenue = 0,
                            RefundAmount = 0
                        };
                    })
    .OrderBy(x => x.Date)
    .ToList();




                return ResultResponse<List<TransactionTotalResponsePerDay>>.SuccessResult(
                    "Get revenue by month successfully", result
                   );
            }
            catch (Exception ex)
            {
                return ResultResponse<List<TransactionTotalResponsePerDay>>.ServerError(
                    $"Error calculating total revenue: {ex.Message}");
            }
        }
        public async Task<ResultResponse<List<TransactionTotalResponsePerDay>>> GetTotalRevevenuePerWeek(DateOnly fromDate, DateOnly toDate)
        {
            try
            {
                var transactions =
                    await _unitOfWork
                        .GetTransactionRepository()
                        .GetTransactionsByVietnamRangeAsync(fromDate, toDate);

                var successTransactions = transactions
                    .Where(t => t.Status == TransactionStatusEnum.Success.ToString())
                    .ToList();

                var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var allDays = Enumerable
    .Range(0, toDate.DayNumber - fromDate.DayNumber + 1)
    .Select(offset => fromDate.AddDays(offset))
    .ToList();

                 var revenueByDate = successTransactions
    .GroupBy(t =>
    {
        var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
            t.CreatedAt.UtcDateTime,
            vnTz
        );
        return DateOnly.FromDateTime(vnTime);
    })
    .ToDictionary(g => g.Key, g =>
    {
        var typeSums = g
            .GroupBy(x => x.TransactionType)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(v => v.Amount)
            );

        var grossRevenue =
              typeSums.GetValueOrDefault(TransactionTypeEnum.BookingDeposit.ToString(), 0)
            + typeSums.GetValueOrDefault(TransactionTypeEnum.BookingAdditionalPayment.ToString(), 0);

        var refundAmount =
              typeSums.GetValueOrDefault(TransactionTypeEnum.BookingRefund.ToString(), 0)
            + typeSums.GetValueOrDefault(TransactionTypeEnum.BookingReturnRefund.ToString(), 0)
            + typeSums.GetValueOrDefault(TransactionTypeEnum.InsuranceClaimRefund.ToString(), 0);

        return new { grossRevenue, refundAmount };
    });
                var result = allDays
                .Select(date =>
                {
                    if (revenueByDate.TryGetValue(date, out var revenue))
                    {
                        return new TransactionTotalResponsePerDay
                        {
                            Date = date,
                            GrossRevenue = revenue.grossRevenue,
                            RefundAmount = revenue.refundAmount
                        };
                    }

                    return new TransactionTotalResponsePerDay
                    {
                        Date = date,
                        GrossRevenue = 0,
                        RefundAmount = 0
                    };
                })
.OrderBy(x => x.Date)
.ToList();




                return ResultResponse<List<TransactionTotalResponsePerDay>>.SuccessResult(
                    "Get revenue by month successfully", result
                   );
            }
            catch (Exception ex)
            {
                return ResultResponse<List<TransactionTotalResponsePerDay>>.ServerError(
                    $"Error calculating total revenue: {ex.Message}");
            }
        }

        public async Task<ResultResponse<TransactionTotalResponsePerDay>> GetTotalRevevenuePerDay(DateOnly day)
        {
            try
            {
                var transactions =
                    await _unitOfWork
                        .GetTransactionRepository()
                        .GetTransactionsByVietnamDayAsync(day);

                var successTransactions = transactions
                    .Where(t => t.Status == TransactionStatusEnum.Success.ToString())
                    .ToList();

                var typeSums = successTransactions
                   .GroupBy(t => t.TransactionType)
                   .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                var grossRevenue =
              typeSums.GetValueOrDefault(TransactionTypeEnum.BookingDeposit.ToString(), 0)
            + typeSums.GetValueOrDefault(TransactionTypeEnum.BookingAdditionalPayment.ToString(), 0);

                var refundAmount =
                      typeSums.GetValueOrDefault(TransactionTypeEnum.BookingRefund.ToString(), 0)
                    + typeSums.GetValueOrDefault(TransactionTypeEnum.BookingReturnRefund.ToString(), 0)
                    + typeSums.GetValueOrDefault(TransactionTypeEnum.InsuranceClaimRefund.ToString(), 0);

                var response= new TransactionTotalResponsePerDay
                {
                    Date = day,
                    GrossRevenue = grossRevenue,
                    RefundAmount = refundAmount
                };

                return ResultResponse<TransactionTotalResponsePerDay>.SuccessResult(
                    "Get revenue by month successfully", response
                   );
            }
            catch (Exception ex)
            {
                return ResultResponse<TransactionTotalResponsePerDay>.ServerError(
                    $"Error calculating total revenue: {ex.Message}");
            }
        }
    }
}
