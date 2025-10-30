using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models;

public class FptResponse<T>
{
    public string? code { get; set; }
    public T? data { get; set; }
    public string? message { get; set; }
}
