using EMRS.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class UpdateReturnReceiptResponse
    {
        public Guid BookingId { get; set; }
        public Guid RentalReceiptId { get; set; }

        public UpdateSummary UpdateSummary { get; set; }

        public SettlementSummary NewSettlement { get; set; }

        public VehicleVerificationResult? NewVerificationResult { get; set; }
        public DamageDetectionResult? NewDamageResult { get; set; }
    }

    public class UpdateSummary
    {
        public bool OdometerUpdated { get; set; }
        public bool BatteryUpdated { get; set; }
        public bool NotesUpdated { get; set; }
        public bool ImagesReplaced { get; set; }
        public int NewImagesCount { get; set; }
        public bool ChecklistReplaced { get; set; }
        public bool AdditionalFeesReplaced { get; set; }
        public bool AIAnalysisRerun { get; set; }
    }

}
