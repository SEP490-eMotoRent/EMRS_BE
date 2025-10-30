using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsuranceClaimDTOs
{
    public class CreateInsuranceClaimRequest
    {
        public Guid BookingId { get; set; }
        public DateTime IncidentDate { get; set; }
        public string IncidentLocation { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public List<IFormFile?>? IncidentImageFiles { get; set; }
    }
}
