using EMRS.Application.Common;
using EMRS.Application.DTOs.RentalContractDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IRentalService
{
    Task<ResultResponse<RentalReceiptResponse>> CreateRentailReceiptAsync(RentalReceiptCreateRequest rentalReceiptCreateRequest);
    Task<ResultResponse<RentalReceiptResponse>> GetAllByBookingIdAsync(Guid bookingId);
    Task<ResultResponse<List<RentalReceiptResponse>>> GetAllRentalReceipt();
    Task<ResultResponse<string>> DeleteRentalReceiptAsync(Guid rentalReceiptId);
    Task<ResultResponse<RentalContractResponse>> GetContractAsync(Guid contractId);
    Task<ResultResponse<RentalContractFileResponse>> CreateRentalContractAsync(Guid BookingId);
    Task<ResultResponse<string>> SendRenterCodeForOtpSignAsync(Guid rentalContractId);
    Task<ResultResponse<string>> ConfirmedRentalReceipt(Guid rentalReceiptId, string otpCode);
}
