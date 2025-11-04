using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.BackgroundJobs.Booking;

public interface IBookingJobScheduler
{
    void ScheduleAutoCancel(Guid bookingId, TimeSpan delay);
}
