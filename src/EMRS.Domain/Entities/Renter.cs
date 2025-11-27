using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Renter:BaseEntity
    {
        public string Email { get; set; }
        public string phone { get; set; }
        public string Address { get; set; }
        public string? DateOfBirth { get; set; }
        public Guid AccountId { get; set; } 
        public Guid MembershipId { get; set; }
        public bool IsVerified { get; set; } = false;

        public string? FaceToken { get; set; }
        public  string VerificationCode { get; set; } = string.Empty;

        public DateTime? VerificationCodeExpiry { get; set; }
        //relationship
        public ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public Account Account { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<GPSSharing> GPSSharings { get; set; } = new List<GPSSharing>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Ticket> Tickets { get; set; }= new List<Ticket>();
        public Wallet? Wallet { get; set; }
        [ForeignKey(nameof(MembershipId))]
        public Membership Membership { get; set; } = null!;
        [InverseProperty(nameof(GPSSharing.OwnerRenter))]
        public ICollection<GPSSharing> OwnerGPSSharings { get; set; } = new List<GPSSharing>();

        // Là người được chia sẻ GPS
        [InverseProperty(nameof(GPSSharing.GuestRenter))]
        public ICollection<GPSSharing> GuestGPSSharings { get; set; } = new List<GPSSharing>();

    }
}
