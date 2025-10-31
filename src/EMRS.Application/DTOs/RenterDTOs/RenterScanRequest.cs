using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RenterDTOs;

public class RenterScanRequest
{
    public IFormFile image { get; set; }

}