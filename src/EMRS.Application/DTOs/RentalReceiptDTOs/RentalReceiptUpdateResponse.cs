using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs;

public class RentalReceiptUpdateResponse
{
    public Guid Id { get; set; }






    public decimal EndBatteryPercentage { get; set; }

    public decimal EndOdometerKm { get; set; }


    public List<string> ReturnVehicleImageFiles { get; set; }

    public string CheckListFile { get; set; }
}
