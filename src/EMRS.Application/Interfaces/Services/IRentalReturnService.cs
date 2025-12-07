using EMRS.Application.Common;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IRentalReturnService
    {
        Task<ResultResponse<ReturnInitResponse>> InitiateReturnProcessAsync(IFormFile faceImage);

        Task<ResultResponse<UploadReturnImagesResponse>> UploadAndAnalyzeReturnImagesAsync(
            UploadReturnImagesRequest request);

        Task<ResultResponse<CreateReturnReceiptResponse>> CreateReturnReceiptAsync(
            CreateReturnReceipt request);

        Task<ResultResponse<FinalizeReturnResponse>> FinalizeReturnAsync(
            FinalizeReturn request);

        Task<ResultResponse<SettlementSummary>> GetSettlementSummaryAsync(Guid bookingId);

        Task<ResultResponse<UpdateReturnReceiptResponse>> UpdateReturnReceiptAsync(UpdateReturnReceiptRequest request);

        Task<ResultResponse<string>> DeleteReturnReceiptAsync(Guid bookingId);

        Task<ResultResponse<ReturnForVehicleSwapResponse>> ReturnForVehicleSwapAsync(ReturnForVehicleSwapRequest request);
    }
}
