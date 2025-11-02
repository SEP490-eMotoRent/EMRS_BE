using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class FinalizeReturnRequest
    {
        public Guid BookingId { get; set; }
        public bool RenterConfirmed { get; set; } // True nếu Renter xác nhận, False nếu Staff tự tạo
    }

    public class FinalizeReturnResponse
    {
        public Guid BookingId { get; set; }
        public string BookingStatus { get; set; }
        public DateTimeOffset ActualReturnDatetime { get; set; }

        public PaymentResult PaymentResult { get; set; }
        public VehicleStatusUpdate VehicleUpdate { get; set; }
    }

    public class PaymentResult
    {
        public decimal RefundAmount { get; set; }
        public string TransactionType { get; set; } // "REFUND" or "ADDITIONAL_PAYMENT"
        public decimal WalletBalanceAfter { get; set; }
    }

    public class VehicleStatusUpdate
    {
        public Guid VehicleId { get; set; }
        public string Status { get; set; }
        public decimal CurrentOdometerKm { get; set; }
        public decimal BatteryHealthPercentage { get; set; }
    }
}
