using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class UploadReturnImagesRequest
    {
        public Guid BookingId { get; set; }
        public List<IFormFile> ReturnImages { get; set; } // 4 ảnh xe

    }
}
