using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Enums
{
    public enum InsuranceClaimStatusEnum
    {
        Reported,      // Renter vừa báo cáo
        Approved,      // Manager đã duyệt, gửi cho bảo hiểm
        Processing,    // Bảo hiểm đang xử lý
        Completed,     // Hoàn tất quyết toán
        Rejected       // Manager từ chối
    }
}
