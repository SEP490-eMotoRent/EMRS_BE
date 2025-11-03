using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models.VNPay;

public class VNPayResponseData
{
    public bool IsSuccess { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string? BankTransactionNo { get; set; }
    public string? CardType { get; set; }
    public DateTime? TransactionDate { get; set; }
    public Dictionary<string, string> RawData { get; set; } = new();
}