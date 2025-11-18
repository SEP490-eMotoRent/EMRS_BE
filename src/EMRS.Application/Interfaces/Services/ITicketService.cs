using EMRS.Application.Common;
using EMRS.Application.DTOs.TicketDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface ITicketService
    {
        Task<ResultResponse<TicketDetailResponse>> GetTicketByIdAsync(Guid ticketId);
        Task<ResultResponse<PaginationResult<List<TicketResponse>>>> GetAllTicketByBookingIdAsync(
  Guid BookingId, int pageSize, int pageNum, bool orderByDescending = true);
        Task<ResultResponse<PaginationResult<List<TicketResponse>>>> GetAllTicketByStaffIdAsync(
    Guid StaffId, int pageSize, int pageNum, bool orderByDescending = true);
        Task<ResultResponse<PaginationResult<List<TicketResponse>>>> GetAllTicketAsync(
     int pageSize, int pageNum, bool orderByDescending = true);
        Task<ResultResponse<TicketDetailResponse>> CreateTicketAsync(TicketCreateRequest ticketCreateRequest);
        Task<ResultResponse<TicketResponse>> UpdateTicketAsync(TicketUpdateRequest ticketUpdateRequest);
    }
}
