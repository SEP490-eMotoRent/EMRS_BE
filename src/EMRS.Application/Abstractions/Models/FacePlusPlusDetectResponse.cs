using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models;

public class FacePlusPlusDetectResponse
{
    public List<face>? faces { get; set; }
    public string? image_id { get; set; }
    public string? request_id { get; set; }
    public int time_used { get; set; }
    public int face_num { get; set; }
}

public class face
{
    public string? face_token { get; set; }
    public face_rectangle? face_rectangle { get; set; }
}

public class face_rectangle
{
    public int top { get; set; }
    public int left { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}


