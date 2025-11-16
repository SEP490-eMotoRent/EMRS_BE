using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models.Protrack;

public class TempTrackingPayload
{
    public Guid vehicleId { get; set; }
    public string imei { get; set; }
    public string deviceId { get; set; }
    public long exp { get; set; }
}
