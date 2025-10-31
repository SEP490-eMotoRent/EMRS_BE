using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models
{
    public class FacePlusPlusSearchResponse
    {
        public string? request_id { get; set; }
        public thresholds? thresholds { get; set; }
        public List<face_result>? results { get; set; }
    }

    public class thresholds
    {
        public double? _1e_3 { get; set; }
        public double? _1e_5 { get; set; }
        public double? _1e_4 { get; set; }
    }

    public class face_result
    {
        public double? confidence { get; set; }
        public string? user_id { get; set; }
        public string? face_token { get; set; }
    }
}
