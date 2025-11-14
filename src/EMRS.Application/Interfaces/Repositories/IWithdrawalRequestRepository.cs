using EMRS.Application.Common;
using EMRS.Application.DTOs.WithdrawalRequestDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IWithdrawalRequestRepository : IGenericRepository<WithdrawalRequest>
    {
        Task<WithdrawalRequest?> GetWithdrawalRequestWithDetailsAsync(Guid id);
        Task<List<WithdrawalRequest>> GetWithdrawalRequestsByWalletIdAsync(Guid walletId);
        Task<List<WithdrawalRequest>> GetWithdrawalRequestsByStatusAsync(string status);
        Task<PaginationResult<List<WithdrawalRequest>>> GetWithdrawalRequestsWithFilter(
            WithdrawalRequestSearchRequest request, int pageSize, int pageNum);
    }
}
