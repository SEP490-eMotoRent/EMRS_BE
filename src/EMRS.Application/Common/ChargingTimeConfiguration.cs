using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EMRS.Application.Common
{
    public class ChargingTimeConfiguration
    {
        [JsonPropertyName("timeRanges")]
        public List<TimeRange> TimeRanges { get; set; } = new();

        [JsonPropertyName("daysOfWeek")]
        public List<int> DaysOfWeek { get; set; } = new(); // 0=Sunday, 1=Monday,...6=Saturday
    }
}
