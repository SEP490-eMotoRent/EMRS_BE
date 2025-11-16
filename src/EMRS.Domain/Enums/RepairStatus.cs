using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Enums
{
    public enum RepairStatus
    {
        Pending,       // Manager mới tạo
        Assigned,      // Admin duyệt
        Completed,     // Tech báo xong
        Cancelled,     // Manager xác nhận
    }
}
