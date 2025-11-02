using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions
{
    public interface IGeminiAIService
    {
        /// <summary>
        /// Xác thực xe: So sánh ảnh handover vs return để xác nhận đúng xe
        /// </summary>
        Task<VehicleVerificationResult> VerifyVehicleAsync(
            List<string> handoverImageUrls,
            List<string> returnImageUrls,
            string licensePlate);

        /// <summary>
        /// Phát hiện hư hỏng: So sánh ảnh để tìm vết xước, móp méo mới
        /// </summary>
        Task<DamageDetectionResult> DetectDamagesAsync(
            List<string> handoverImageUrls,
            List<string> returnImageUrls);
    }

    // Response Models
    public class VehicleVerificationResult
    {
        public bool IsVerified { get; set; }
        public double Confidence { get; set; }
        public string Reason { get; set; }
        public string LicensePlateMatch { get; set; } // "MATCH" | "MISMATCH" | "UNCLEAR"
    }

    public class DamageDetectionResult
    {
        public bool HasNewDamages { get; set; }
        public List<DamageSuggestion> Suggestions { get; set; }
    }

    public class DamageSuggestion
    {
        public string Location { get; set; } // "Front bumper", "Left side", etc.
        public string DamageType { get; set; } // "Scratch", "Dent", "Missing part"
        public string Severity { get; set; } // "Minor", "Moderate", "Severe"
        public double Confidence { get; set; }
        public string Description { get; set; }
    }
}
