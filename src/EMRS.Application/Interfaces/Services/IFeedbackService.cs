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
    }
}
