using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Common;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo VietnamTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    public static DateTimeOffset? NormalizeToUtc(DateTimeOffset? dateTime)
        => dateTime?.ToUniversalTime();

    public static DateTime? NormalizeToUtc(DateTime? dateTime)
        => dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : null;
    public static DateTimeOffset? ToVietnamTime(DateTimeOffset? utcTime)
            => utcTime.HasValue ? TimeZoneInfo.ConvertTime(utcTime.Value, VietnamTimeZone) : null;


    public static DateTime? ToVietnamTime(DateTime? utcTime)
        => utcTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime.Value, DateTimeKind.Utc), VietnamTimeZone) : null;
}
