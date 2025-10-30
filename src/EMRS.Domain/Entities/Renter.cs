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

        public string? FaceVerificationId { get; set; }
        public  string VerificationCode { get; set; } = string.Empty;

        public DateTime? VerificationCodeExpiry { get; set; }
        //relationship
        public ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public Account Account { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public Wallet? Wallet { get; set; }
        [ForeignKey(nameof(MembershipId))]
        public Membership Membership { get; set; } = null!;

    }
}
