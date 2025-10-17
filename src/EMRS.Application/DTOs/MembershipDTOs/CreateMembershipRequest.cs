using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.MembershipDTOs;

public class CreateMembershipRequest
{

    
     public string TierName { get; set; }
     public decimal MinBookings { get; set; }
       public decimal DiscountPercentage { get; set; }
      public  decimal FreeChargingPerMonth { get; set; }
      public  string Description { get; set; }

  
}


