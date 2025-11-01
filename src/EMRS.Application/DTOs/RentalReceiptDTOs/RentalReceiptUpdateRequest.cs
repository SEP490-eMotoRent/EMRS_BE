using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs;

public class RentalReceiptUpdateRequest
{
    public Guid RentalReceiptId { get; set; }
    public decimal EndOdometerKm { get; set; }
    public decimal EndBatteryPercentage { get; set; }
    public List<IFormFile> ReturnVehicleImagesFiles { get; set; }
    public IFormFile ReturnCheckListFile { get; set; }
}
