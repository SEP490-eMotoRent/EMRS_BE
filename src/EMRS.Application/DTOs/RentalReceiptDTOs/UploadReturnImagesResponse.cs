using EMRS.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class UploadReturnImagesResponse
    {
        public List<string> UploadedImageUrls { get; set; }
        public VehicleVerificationResult VerificationResult { get; set; }
        public DamageDetectionResult DamageResult { get; set; }
    }
}
