using EMRS.Application.DTOs.BookingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalContractDTOs;

public class RentalContractResponse
{
    public Guid Id { get; set; }
    public string OtpCode { get; set; }
    public DateTime? ExpireAt { get; set; }
    public string ContractStatus { get; set; }
/*    public BookingResponse booking {  get; set; }
*/    public string file {  get; set; }
}
