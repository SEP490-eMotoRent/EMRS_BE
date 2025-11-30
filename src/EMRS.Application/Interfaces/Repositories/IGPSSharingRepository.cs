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

        Task<GPSSharing?> GetActiveSessionByRenterIdAsync(Guid renterId);

       /* Task<GPSSharing?> GetPendingInvitationByVehicleIdAsync(Guid vehicleId);*/

        Task<List<GPSSharing>> GetSessionsByRenterIdAsync(Guid renterId);

        Task<List<GPSSharing>> GetAllSessionsForHistoryAsync(); // For Manager/Admin

      /*  Task<GPSSharing?> GetSessionWithDetailsAsync(Guid sessionId);*/
    }
}
