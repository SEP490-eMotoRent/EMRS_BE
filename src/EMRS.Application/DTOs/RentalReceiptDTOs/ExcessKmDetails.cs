using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class ExcessKmDetails
    {
        public decimal TotalKmLimit { get; set; }
        public decimal ActualKmDriven { get; set; }
        public decimal ExcessKm { get; set; }
        public decimal RatePerKm { get; set; }
        public decimal StartOdometerKm { get; set; }
        public decimal EndOdometerKm { get; set; }
    }
}
