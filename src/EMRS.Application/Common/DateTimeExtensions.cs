using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Common;

public static class DateTimeExtensions
{
    public static string ToVietnamTimeString(this DateTime? utcDateTime, string format = "dd/MM/yyyy HH:mm")
    {
        if (utcDateTime == null)
            return string.Empty;

        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // UTC+7
        var vnDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime.Value, DateTimeKind.Utc), vnTimeZone);
        return vnDateTime.ToString(format);
    }
}

