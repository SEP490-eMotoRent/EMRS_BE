using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IGPSSharingRepository : IGenericRepository<GPSSharing>
    {
        Task<GPSSharing?> GetByInvitationCodeAsync(string invitationCode);

        Task<GPSSharing?> GetActiveSessionByBookingIdAsync(Guid bookingId);

        Task<GPSSharing?> GetActiveSessionByRenterIdAsync(Guid renterId);

        Task<GPSSharing?> GetPendingInvitationByBookingIdAsync(Guid bookingId);

        Task<List<GPSSharing>> GetAllSessionsForHistoryAsync(); // For Manager/Admin

        Task<GPSSharing?> GetSessionWithDetailsAsync(Guid sessionId);
    }
}
