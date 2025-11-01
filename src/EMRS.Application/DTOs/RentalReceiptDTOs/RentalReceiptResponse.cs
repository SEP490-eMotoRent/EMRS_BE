using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs;

public class RentalReceiptResponse
{




    public Guid Id { get; set; }



    public string? Notes { get; set; }
    public DateTime? RenterConfirmedAt { get; set; }
    public decimal StartOdometerKm { get; set; }
    public decimal StartBatteryPercentage { get; set; }
    public Guid BookingId { get; set; }
    public Guid StaffId { get; set; }
    public decimal EndBatteryPercentage { get; set; }

    public decimal EndOdometerKm { get; set; }

    public List<string>? HandOverVehicleImageFiles { get; set; } = new List<string>();
    public List<string>? ReturnVehicleImageFiles { get; set; }= new List<string>();

    public List<string>? CheckListFile { get; set; } = new List<string>();
}

