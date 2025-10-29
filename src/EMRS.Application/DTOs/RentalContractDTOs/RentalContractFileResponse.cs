using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalContractDTOs;

public class RentalContractFileResponse
{
    public byte[] FileData { get; set; }
    public string Name { get; set; }
}
