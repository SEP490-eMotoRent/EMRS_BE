using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleDTOs
{
    public class VehicleTotalResponse
    {
        public int TotalVehicles { get; set; }
        public int TotalAvailable { get; set; }
        public int TotalBooked { get; set; }
        public int TotalHold { get; set; }
        public int TotalTransfering { get; set; }
        public int TotalRented { get; set; }
        public int TotalUnavailable { get; set; }
        public int TotalRepaired { get; set; }

        public int TotalTracked { get; set; }
    }
}
