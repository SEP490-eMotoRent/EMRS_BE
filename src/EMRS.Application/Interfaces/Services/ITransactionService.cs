using EMRS.Application.Common;
using EMRS.Application.DTOs.TransactionDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface ITransactionService
    {
        Task<ResultResponse<List<TransactionTotalResponsePerDay>>> GetTotalRevevenuePerWeek(DateOnly fromDate, DateOnly toDate);
        Task<ResultResponse<TransactionTotalResponse>> GetTotalRevenueAsync();
        Task<ResultResponse<TransactionTotalResponsePerDay>> GetTotalRevevenuePerDay(DateOnly day);
        Task<ResultResponse<List<TransactionResponse>>> GetAllAsync();
        Task<ResultResponse<TransactionResponse>> GetByIdAsync(Guid id);
        Task<ResultResponse<TransactionResponse>> CreateAsync(TransactionCreateRequest request);
        Task<ResultResponse<TransactionResponse>> UpdateAsync(Guid id, TransactionUpdateRequest request);
        Task<ResultResponse<TransactionResponse>> DeleteAsync(Guid id);
        Task<ResultResponse<List<TransactionTotalResponsePerDay>>> GetTotalRevevenuePerMonth(int year, int month);
        Task<ResultResponse<List<TransactionResponse>>> GetByRenterIdAsync(Guid renterId);
        Task<ResultResponse<TransactionTotalResponseMonths>> GetTotalRevevenueMonths(int year);
    }
}
