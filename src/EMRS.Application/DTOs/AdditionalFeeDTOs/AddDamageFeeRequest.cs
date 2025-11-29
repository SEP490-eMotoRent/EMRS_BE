using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AdditionalFeeDTOs
{
    public class AddDamageFeeRequest
    {
        public Guid BookingId { get; set; }
        public string DamageType { get; set; } // "Gương vỡ/mất", "Đèn vỡ", etc.
        public decimal Amount { get; set; }
        public string? AdditionalNotes { get; set; } // Staff tự ghi chú thêm
    }
}
