using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.BackgroundJobs.GPSSharing
{
    public interface IGPSSharingJobScheduler
    {
        void ScheduleAutoExpire(Guid sessionId, TimeSpan delay);
    }
}
