using EMRS.Application.Abstractions.BackgroundJobs.Booking;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.BackgroundJobs.Booking
{
    public class BookingJobScheduler: IBookingJobScheduler
    {
        public void ScheduleAutoCancel(Guid bookingId, TimeSpan delay)
        {
            BackgroundJob.Schedule<BookingBackgroundJob>(
                job => job.AutoCancelPendingBooking(bookingId),
                delay
            );

            Console.WriteLine($"[Hangfire] Scheduled auto-cancel for Booking {bookingId} after {delay.TotalMinutes} minutes.");
        }
    }
}
