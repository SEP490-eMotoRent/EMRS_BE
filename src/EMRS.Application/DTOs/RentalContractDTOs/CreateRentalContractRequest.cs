using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalContractDTOs
{
    public class CreateRentalContractRequest
    {
        public Guid RentalReceiptId { get; set; }
        public Guid BookingId { get; set; }
    }
}
