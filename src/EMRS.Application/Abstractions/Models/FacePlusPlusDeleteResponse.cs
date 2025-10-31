using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models;

public class FacePlusPlusDeleteResponse
{
    public int? time_used { get; set; }
    public string? faceset_token { get; set; }
    public string? outer_id { get; set; }
    public string? request_id { get; set; }
}
