using EMRS.Application.Abstractions.BackgroundJobs.GPSSharing;
using EMRS.Application.Interfaces.Services;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.BackgroundJobs.GPSSharing
{
    public class GPSSharingJobScheduler : IGPSSharingJobScheduler
    {
        public void ScheduleAutoExpire(Guid sessionId, TimeSpan delay)
        {
            BackgroundJob.Schedule<GPSSharingBackgroundJob>(
                job => job.AutoExpireInvitation(sessionId),
                delay
            );

            Console.WriteLine($"[Hangfire] Scheduled auto-expire for GPSSharing {sessionId} after {delay.TotalMinutes} minutes.");
        }
    }
}
