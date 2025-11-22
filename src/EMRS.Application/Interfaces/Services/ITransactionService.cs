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
        Task<ResultResponse<List<TransactionResponse>>> GetAllAsync();
        Task<ResultResponse<TransactionResponse>> GetByIdAsync(Guid id);
        Task<ResultResponse<TransactionResponse>> CreateAsync(TransactionCreateRequest request);
        Task<ResultResponse<TransactionResponse>> UpdateAsync(Guid id, TransactionUpdateRequest request);
        Task<ResultResponse<TransactionResponse>> DeleteAsync(Guid id);
        Task<ResultResponse<List<TransactionResponse>>> GetByRenterIdAsync(Guid renterId);
    }
}
