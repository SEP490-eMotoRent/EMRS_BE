using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs;

public class RentalReceiptResponse
{



    public decimal EndOdometerKm { get; set; }




    public decimal OdometerReading { get; set; }
    public decimal BatteryPercentage { get; set; }
    public string? Notes { get; set; }
    public DateTime? RenterConfirmedAt { get; set; }
    public decimal StartOdometerKm { get; set; }
    public decimal StartBatteryPercentage { get; set; }
    public Guid BookingId { get; set; }
    public Guid StaffId { get; set; }

    public List<string>? VehicleFiles { get; set; }

    public string CheckListFile { get; set; }
}

