using EMRS.Application.Common;
using EMRS.Application.DTOs.GPSSharingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IGPSSharingService
    {
        // For RENTER
        Task<ResultResponse<GPSSharingInviteResponse>> CreateInvitation(
            GPSSharingCreateRequest request);

        Task<ResultResponse<GPSSharingActiveResponse>> JoinSharing(
            GPSSharingJoinRequest request);

        Task<ResultResponse<GPSSharingSessionResponse>> GetSessionDetail(Guid sessionId);

        Task<ResultResponse<bool>> CancelSession(Guid sessionId);
        Task<ResultResponse<List<GPSSharingHistoryResponse>>> GetAllSessions();
    }
}
