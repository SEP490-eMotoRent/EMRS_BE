using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.MediaDTOs
{
    public class MediaUpdateRequest
    {
        public Guid MediaId { get; set; }          // Đổi tên cho đúng nghĩa
        public IFormFile File { get; set; }
    }
}
