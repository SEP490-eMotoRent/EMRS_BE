using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.BackgroundJobs.Booking;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.BackgroundJobs.Booking
{
    public class BookingBackgroundJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingBackgroundJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AutoCancelPendingBooking(Guid bookingId)
        {
            try
            {
                var booking = await _unitOfWork.GetBookingRepository().FindByIdAsync(bookingId);
                if (booking == null)
                {
                    Console.WriteLine($"[Hangfire] Booking {bookingId} not found");
                    return;
                }

                if (booking.BookingStatus == BookingStatusEnum.Pending.ToString())
                {
                    booking.BookingStatus = BookingStatusEnum.Canceled.ToString();
                    var vehicle = _unitOfWork.GetVehicleRepository()
                        .GetRandomVehicleAsync(booking.VehicleModelId).Result
                        .FirstOrDefault(v => v.Status == VehicleStatusEnum.Hold.ToString());



                    vehicle.Status = VehicleStatusEnum.Available.ToString();

                    _unitOfWork.GetVehicleRepository().Update(vehicle);
                    await _unitOfWork.SaveChangesAsync();
                    Console.WriteLine($"[Hangfire] Booking {bookingId} auto-cancelled successfully");
                }
                else
                {
                    Console.WriteLine($"[Hangfire] Booking {bookingId} already updated, skipping");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hangfire] Error auto-cancelling Booking {bookingId}: {ex.Message}");
            }
        }
    }
}
