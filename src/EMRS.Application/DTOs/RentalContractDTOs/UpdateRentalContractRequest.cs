using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalContractDTOs
{
    public class UpdateRentalContractRequest
    {
        public Guid RentalContractId { get; set; }

        public IFormFile? ContractFile { get; set; }

    }
}
