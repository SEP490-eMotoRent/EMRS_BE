using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models;

public class FacePlusPlusRemoveFaceResponse
{
    public string request_id { get; set; } 

    public string faceset_token { get; set; } 

    public string outer_id { get; set; } 

    public int face_removed { get; set; }

    public int face_count { get; set; }

    public List<FailureDetail> failure_detail { get; set; } = new();

    public int time_used { get; set; }

    public string? error_message { get; set; }
}

public class FailureDetail
{
    public string face_token { get; set; }

    public string reason { get; set; } 
}
