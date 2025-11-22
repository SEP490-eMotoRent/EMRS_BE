using EMRS.Application.Common;
using EMRS.Application.DTOs.RentalContractDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IRentalService
{
    Task<ResultResponse<RentalContractResponse>> CreateRentalContractAsync(
    RentalContractCreateRequest request);
    Task<ResultResponse<RentalContractResponse>> UpdateRentalContractAsync(
    UpdateRentalContractRequest request);
    Task<ResultResponse<RentalContractFileResponse>> CreateRentalContractPdfByGenerateAsync(Guid Booking, Guid RentalReceiptId);
    Task<ResultResponse<RentalReceiptCreateResponse>> CreateRentailReceiptForChangingAsync(RentalReceiptCreateVehicleChangingRequest rentalReceiptCreateRequest);
    Task<ResultResponse<RentalReceiptResponse>> GetRentalReceiptDetailByIdAsync(Guid rentalReceiptId);
    Task<ResultResponse<RentalReceiptCreateResponse>> CreateRentailReceiptAsync(RentalReceiptCreateRequest rentalReceiptCreateRequest);
    Task<ResultResponse<List<RentalReceiptListResponse>>> GetRentalReceiptDetailByBookingIdAsync(Guid bookingId);
    Task<ResultResponse<List<RentalReceiptResponse>>> GetAllRentalReceipt();
    Task<ResultResponse<string>> DeleteRentalReceiptAsync(Guid rentalReceiptId);
    Task<ResultResponse<RentalContractResponse>> GetContractAsync(Guid bookingId);
    Task<ResultResponse<RentalContractFileResponse>> CreateRentalContractWithPDFQuestAsync(Guid BookingId, Guid RentalReceiptId);
    Task<ResultResponse<string>> SendRenterCodeForOtpSignAsync(Guid rentalContractId);
    Task<ResultResponse<string>> ConfirmedRentalContract(Guid rentalContractId, Guid rentalReceiptId, string otpCode);
    Task<ResultResponse<string>> DeleteContractAsync(Guid contractId);
    Task<ResultResponse<List<RentalContractResponse>>> GetAllRentalContractsAsync();
}
