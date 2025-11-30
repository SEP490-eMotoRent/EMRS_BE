using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.StaffDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class RentalReceiptDetailResponse
    {
        public Guid Id { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public string? Notes { get; set; }
        public DateTime? RenterConfirmedAt { get; set; }
        public decimal StartOdometerKm { get; set; }
        public decimal StartBatteryPercentage { get; set; }
        public Guid BookingId { get; set; }
        public Guid StaffId { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public Guid VehicleId { get; set; }
        public decimal EndOdometerKm { get; set; }
        public RentalReceiptStaffResponse Staff { get; set; }

        public List<string>? HandOverVehicleImageFiles { get; set; } = new List<string>();
        public List<string>? ReturnVehicleImageFiles { get; set; } = new List<string>();

        public List<string>? CheckListHandoverFile { get; set; } = new List<string>();
        public List<string>? CheckListReturnFile { get; set; } = new List<string>();
    }
    public class RentalReceiptStaffResponse
    {
        public Guid Id;
        public BranchResponse Branch { get; set; }

        public AccountResponse  Account { get; set; }
    }
}
