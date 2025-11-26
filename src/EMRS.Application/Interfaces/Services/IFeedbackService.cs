using EMRS.Application.Common;
using EMRS.Application.DTOs.FeedbackDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IFeedbackService
    {
        Task<ResultResponse<FeedbackResponse>> CreateFeedback(FeedbackRequest feedbackRequest);
        Task<ResultResponse<List<FeedbackDetailResponse>>> GetFeedbackByVehicleModelId(Guid vehicleModelId);
        Task<ResultResponse<List<FeedbackDetailResponse>>> GetAllFeedbacks();
        Task<ResultResponse<List<FeedbackDetailResponse>>> GetFeedbackByBookingId(Guid bookingId);
    }
}
