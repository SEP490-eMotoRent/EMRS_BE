using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Common;

public class ContractData
{
    public string LessorCompany { get; set; } = "Công ty EMRS";

    public string LessorDeliveryStaffName { get; set; }
    public string LessorDeliveryStaffPosition { get; set; }
    public string DeliveryLocationName { get; set; }

    // === BÊN NHẬN XE (CỐ ĐỊNH) ===
    public string LesseeDriverName { get; set; } 
    public string LesseeDriverId { get; set; }
    public string LesseeDriverPhone { get; set; }

    // === DỮ LIỆU TỪ REQUEST ===
    public string ContractDate { get; set; }
    public string ContractLocation { get; set; }
    public string VehicleModelName { get; set; }
  
    public string VehicleColor { get; set; } 

    public string LicensePlate { get; set; }
    public string RegistrationIssueDate { get; set; }
    public string RentalDay { get; set; }
    public decimal RentalPrice { get; set; }
   
}
