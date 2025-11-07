using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.BranchDTOs;

public class BranchSearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
