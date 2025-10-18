using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalPricingDTOs;

public class RentalPricingResponse
{
    public Guid Id { get; set; }
    public decimal RentalPrice { get; set; }


    public decimal ExcessKmPrice { get; set; }
}
