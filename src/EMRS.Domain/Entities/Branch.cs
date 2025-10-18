using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Branch:BaseEntity
    {
        public string BranchName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OpeningTime { get; set; } = string.Empty;
        public string ClosingTime { get; set; } = string.Empty;



        //relationship  
        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();   
        public ICollection<ChargingRecord> ChargingRecords { get; set; } = new List<ChargingRecord>();
        [InverseProperty(nameof(VehicleTransferOrder.FromBranch))]
        public ICollection<VehicleTransferOrder> SentTransferOrders { get; set; } = new List<VehicleTransferOrder>();

        [InverseProperty(nameof(VehicleTransferOrder.ToBranch))]
        public ICollection<VehicleTransferOrder> ReceivedTransferOrders { get; set; } = new List<VehicleTransferOrder>();


        [InverseProperty(nameof(Booking.HandoverBranch))]
        public ICollection<Booking> HandoverBookings { get; set; } = new List<Booking>();

        [InverseProperty(nameof(Booking.ReturnBranch))]
        public ICollection<Booking> ReturnBookings { get; set; } = new List<Booking>();


    }
}
