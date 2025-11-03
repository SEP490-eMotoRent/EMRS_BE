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
        /// <summary>
        /// API 1: Scan face và khởi tạo quy trình return
        /// </summary>
        Task<ResultResponse<ReturnInitResponse>> InitiateReturnProcessAsync(IFormFile faceImage);

        /// <summary>
        /// API 2: Upload ảnh return và phân tích AI
        /// </summary>
        Task<ResultResponse<UploadReturnImagesResponse>> UploadAndAnalyzeReturnImagesAsync(
            UploadReturnImagesRequest request);

        /// <summary>
        /// API 3: Tạo biên bản trả xe với chi phí
        /// </summary>
        Task<ResultResponse<CreateReturnReceiptResponse>> CreateReturnReceiptAsync(
            CreateReturnReceiptRequest request);

        /// <summary>
        /// API 4: Hoàn tất trả xe và thanh toán
        /// </summary>
        Task<ResultResponse<FinalizeReturnResponse>> FinalizeReturnAsync(
            FinalizeReturnRequest request);

        /// <summary>
        /// API 5: Lấy tóm tắt quyết toán
        /// </summary>
        Task<ResultResponse<SettlementSummary>> GetSettlementSummaryAsync(Guid bookingId);

        Task<ResultResponse<UpdateReturnReceiptResponse>> UpdateReturnReceiptAsync(UpdateReturnReceiptRequest request);
    }
}
