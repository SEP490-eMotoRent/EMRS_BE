using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.MediaDTOs
{
    public class AddManyMediaRequest
    {
        public List<Guid> DocNo { get; set; } = new();
        public List<MediaTypeEnum> MediaType { get; set; } = new();
        public List<MediaEntityTypeEnum> EntityType { get; set; } = new();
        public List<IFormFile> Files { get; set; } = new();
    }
}
