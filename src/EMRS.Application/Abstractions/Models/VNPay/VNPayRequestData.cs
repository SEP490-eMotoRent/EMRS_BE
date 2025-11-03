using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models.VNPay;

public class VNPayRequestData
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderDescription { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string? Locale { get; set; } = "vn";
    public string IpAddress { get; set; } = "127.0.0.1";
    public DateTime? ExpireDate { get; set; }
}
