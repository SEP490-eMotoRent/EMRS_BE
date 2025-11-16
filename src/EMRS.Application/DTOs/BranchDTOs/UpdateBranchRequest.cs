using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.BranchDTOs
{
    public class UpdateBranchRequest
    {
        public string? BranchName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }
    }
}
