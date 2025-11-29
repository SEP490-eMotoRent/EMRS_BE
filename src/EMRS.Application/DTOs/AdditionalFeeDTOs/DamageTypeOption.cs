using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AdditionalFeeDTOs
{
    public class DamageTypeOption
    {
        public string DamageType { get; set; } // "Gương vỡ/mất"
        public string Description { get; set; } // "Hư hỏng trung bình"
        public decimal MinAmount { get; set; } // 200000
        public decimal MaxAmount { get; set; } // 400000
        public string DisplayText { get; set; } // "Gương vỡ/mất (200.000đ - 400.000đ)"
    }
}
