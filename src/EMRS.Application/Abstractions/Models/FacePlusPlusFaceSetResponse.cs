using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models
{
    public class FacePlusPlusFaceSetResponse
    {
        public string? faceset_token { get; set; }
        public int face_added { get; set; }
        public int face_count { get; set; }
        public string? outer_id { get; set; }
        public List<failure_detail>? failure_detail { get; set; }
    }

    public class failure_detail
    {
        public string? reason { get; set; }
        public string? face_token { get; set; }
    }
}
