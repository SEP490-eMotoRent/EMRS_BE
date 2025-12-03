using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Helper;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo VietnamTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    public static DateTimeOffset? NormalizeToUtc(DateTimeOffset? dateTime)
        => dateTime?.ToUniversalTime();
    public static DateTime ToUtc(DateTime vietnamTime)
    {
        var unspecified = DateTime.SpecifyKind(vietnamTime, DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(unspecified, VietnamTimeZone);
    }
    public static DateTime? NormalizeToUtc(DateTime? dateTime)
        => dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : null;
    public static DateTimeOffset? ToVietnamTime(DateTimeOffset? utcTime)
            => utcTime.HasValue ? TimeZoneInfo.ConvertTime(utcTime.Value, VietnamTimeZone) : null;

    public static long GetTimeStamp(DateTime date)
    {
        return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
    }

    public static long GetTimeStamp()
    {
        return GetTimeStamp(DateTime.Now);
    }
    public static DateTime? ToVietnamTime(DateTime? utcTime)
        => utcTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime.Value, DateTimeKind.Utc), VietnamTimeZone) : null;
}
