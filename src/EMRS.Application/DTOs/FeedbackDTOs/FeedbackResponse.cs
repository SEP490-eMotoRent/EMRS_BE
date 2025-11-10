using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.FeedbackDTOs
{
    public class FeedbackResponse
    {
        public Guid FeedbackId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; } 

        public Guid RenterId { get; set; }

        public Guid BookingId { get; set; }
    }
}
