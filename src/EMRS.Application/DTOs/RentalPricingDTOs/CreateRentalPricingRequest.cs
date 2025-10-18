using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalPricingDTOs;

public class CreateRentalPricingRequest
{
    public decimal RentalPrice { get; set; }

    public decimal ExcessKmPrice { get; set; }
}
