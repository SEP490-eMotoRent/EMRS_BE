using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalContractDTOs;

public class RentalContractCreateRequest
{
    public  Guid BookingId { get; set; }
    public IFormFile ContractFile { get; set; }
}
