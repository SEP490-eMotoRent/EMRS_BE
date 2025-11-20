using EMRS.Application.Common;
using EMRS.Application.DTOs.WithdrawalRequestDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IWithdrawalRequestService
    {
        // Renter actions
        Task<ResultResponse<WithdrawalRequestResponse>> CreateWithdrawalRequest(WithdrawalRequestCreateRequest request);
        Task<ResultResponse<List<WithdrawalRequestResponse>>> GetMyWithdrawalRequests();
        Task<ResultResponse<WithdrawalRequestDetailResponse>> GetWithdrawalRequestDetail(Guid id);
        Task<ResultResponse<WithdrawalRequestResponse>> CancelWithdrawalRequest(Guid id);

        // Admin actions
        Task<ResultResponse<PaginationResult<List<WithdrawalRequestDetailResponse>>>> GetAllWithdrawalRequests(
             WithdrawalRequestSearchRequest request, int pageNum, int pageSize);
        Task<ResultResponse<WithdrawalRequestResponse>> ApproveWithdrawalRequest(Guid id);
        Task<ResultResponse<WithdrawalRequestResponse>> RejectWithdrawalRequest(Guid id, string rejectionReason);
        Task<ResultResponse<WithdrawalRequestResponse>> CompleteWithdrawalRequest(Guid id);
    }
}
