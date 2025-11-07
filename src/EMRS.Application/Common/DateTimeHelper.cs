using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Common;

public static class DateTimeHelper
{
    public static DateTimeOffset? NormalizeToUtc(DateTimeOffset? dateTime)
        => dateTime?.ToUniversalTime();

    public static DateTime? NormalizeToUtc(DateTime? dateTime)
        => dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : null;
}
