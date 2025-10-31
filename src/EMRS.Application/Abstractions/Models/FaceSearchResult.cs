using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models
{
    public class FaceSearchResult
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public double Score { get; set; }
    }
}
