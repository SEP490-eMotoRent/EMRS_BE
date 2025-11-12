using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Staff:BaseEntity
    {
        public Guid AccountId { get; set; }
        public Guid? BranchId { get; set; }

        //relationship
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = null!;
        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }
        public ICollection<RentalReceipt> RentalReceipts { get; set; } = new List<RentalReceipt>();

        public ICollection<RepairRequest> RepairRequests { get; set; }= new List<RepairRequest>();

    

        public ICollection<Ticket> Tickets { get; set; }= new List<Ticket>();
        public ICollection<VehicleTransferRequest> vehicleTransferRequests { get; set; } = new List<VehicleTransferRequest>();

        public ICollection<ChargingRecord> ChargingRecords { get; set; } = new List<ChargingRecord>();
    }
}
