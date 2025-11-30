using EMRS.Application.Abstractions;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.BackgroundJobs.GPSSharing
{
    public class GPSSharingBackgroundJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public GPSSharingBackgroundJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AutoExpireInvitation(Guid sessionId)
        {
            try
            {
                var sharing = await _unitOfWork.GetGPSSharingRepository()
                    .FindByIdAsync(sessionId);

                if (sharing == null)
                {
                    Console.WriteLine($"[Hangfire] GPSSharing {sessionId} not found");
                    return;
                }

                
                if (sharing.Status == GPSSharingStatusEnum.Pending.ToString())
                {
                    sharing.Status = GPSSharingStatusEnum.Expired.ToString();
                    _unitOfWork.GetGPSSharingRepository().Update(sharing);
                    await _unitOfWork.SaveChangesAsync();

                    Console.WriteLine($"[Hangfire] GPSSharing invitation {sessionId} auto-expired successfully");
                }
                else
                {
                    Console.WriteLine($"[Hangfire] GPSSharing {sessionId} already {sharing.Status}, skipping");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hangfire] Error auto-expiring GPSSharing {sessionId}: {ex.Message}");
            }
        }
    }
}
