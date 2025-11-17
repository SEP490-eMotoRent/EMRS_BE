using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.MediaDTOs
{
    public class AddMediaRequest
    {
        public Guid DocNo { get; set; } // ID entity liên quan
        public IFormFile File { get; set; } // File upload
        public MediaTypeEnum MediaType { get; set; } = MediaTypeEnum.Image; // Loại media
        public MediaEntityTypeEnum EntityType { get; set; } // Loại entity (Vehicle, VehicleModel...)
    }
}
