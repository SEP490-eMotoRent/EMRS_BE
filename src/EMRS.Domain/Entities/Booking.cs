using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Booking:BaseEntity
    {
        public DateTime? StartDatetime { get; set; }
        public DateTime? EndDatetime { get; set; }
        public DateTime? ActualReturnDatetime { get; set; }
        public decimal BaseRentalFee { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal RentalDays { get; set; }
        public decimal RentalHours { get; set; }
        public decimal RentingRate { get; set; }
        public decimal LateReturnFee { get; set; }
        public decimal AverageRentalPrice { get; set; }
        public decimal ExcessKmFee { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal CrossBranchFee { get; set; }
        public decimal TotalChargingFee { get; set; }
        public decimal TotalAdditionalFee { get; set; }
        public decimal TotalRentalFee { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string BookingStatus { get; set; }

        public Guid VehicleModelId { get; set; }
        public Guid RenterId { get; set; }
        public Guid? VehicleId { get; set; } 

        public Guid? InsurancePackageId { get; set; }
        public Guid? HandoverBranchId { get; set; }
        public Guid? ReturnBranchId { get; set; }

        //relationship
        [ForeignKey(nameof(HandoverBranchId))]
        [InverseProperty(nameof(Branch.HandoverBookings))]
        public Branch? HandoverBranch { get; set; }

        [ForeignKey(nameof(ReturnBranchId))]
        [InverseProperty(nameof(Branch.ReturnBookings))]
        public Branch? ReturnBranch { get; set; }

        [ForeignKey(nameof(RenterId))]
        public Renter Renter { get; set; } = null!;
        [ForeignKey(nameof(VehicleModelId))]
        public VehicleModel VehicleModel { get; set; } = null!;

        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; } = null!;

        [ForeignKey(nameof(InsurancePackageId))]
        public InsurancePackage? InsurancePackage { get; set; }
        public RentalContract? RentalContract { get; set; }
        public RentalReceipt? RentalReceipt { get; set; }

        public Feedback? Feedback { get; set; }


        public InsuranceClaim? InsuranceClaim { get; set; }


        public ICollection<ChargingRecord> ChargingRecords { get; set; } = new List<ChargingRecord>();
        public ICollection<AdditionalFee> AdditionalFees { get; set; } = new List<AdditionalFee>();


    }
}
