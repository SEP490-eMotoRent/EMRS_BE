using EMRS.Application.DTOs.MediaDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleModelDTOs
{
   
    public class VehicleModelListManageResponse
    {
        public Guid VehicleModelId { get; set; }
        public string ModelName { get; set; }
        public string Category { get; set; }
        public decimal BatteryCapacityKwh { get; set; }
        public decimal MaxRangeKm { get; set; }
        public decimal RentalPrice { get; set; }
        public List<MediaResponse> medias { get; set; }
        public decimal OriginalRentalPrice { get; set; }
        public List<ColorResponse> AvailableColors { get; set; }
      

    }
   
}
