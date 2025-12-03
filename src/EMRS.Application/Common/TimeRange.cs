using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EMRS.Application.Common
{
    public class TimeRange
    {
        [JsonPropertyName("start")]
        public string Start { get; set; } = string.Empty; // Format: "HH:mm"

        [JsonPropertyName("end")]
        public string End { get; set; } = string.Empty; // Format: "HH:mm"


        // Kiểm tra xem một thời điểm có nằm trong khung giờ này không
        
        public bool IsInRange(int totalMinutes)
        {
            var startParts = Start.Split(':');
            var endParts = End.Split(':');

            var startMinutes = int.Parse(startParts[0]) * 60 + int.Parse(startParts[1]);
            var endMinutes = int.Parse(endParts[0]) * 60 + int.Parse(endParts[1]);

            // Xử lý trường hợp khung giờ qua nửa đêm (VD: 22:00 - 04:00)
            if (endMinutes < startMinutes)
            {
                // Chia thành 2 khung: start -> 23:59 và 00:00 -> end
                return totalMinutes >= startMinutes || totalMinutes < endMinutes;
            }
            else
            {
                return totalMinutes >= startMinutes && totalMinutes < endMinutes;
            }
        }

    }
}
