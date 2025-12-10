using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class UpdateRentalReceiptHandoverRequest
    {
        public Guid Id { get; set; }
        public string? Notes { get; set; }

        public decimal StartOdometerKm { get; set; }
        public decimal StartBatteryPercentage { get; set; }
        

        public List<IFormFile>? VehicleFiles { get; set; }

        public IFormFile CheckListFile { get; set; }
    }
}
