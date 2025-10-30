using EMRS.Application.DTOs.BranchDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.StaffDTOs;

public class StaffResponse
{
    public Guid Id { get; set; }
    public BranchResponse Branch { get; set; }
}
