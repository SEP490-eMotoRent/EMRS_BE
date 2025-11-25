using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.FeedbackDTOs;
using EMRS.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class FeedbackService:IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public FeedbackService(ICurrentUserService currentUserService,IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        public async Task<ResultResponse<List<FeedbackResponse>>> GetAllFeedbacks()
        {
            try
            {
                var feedbacks = await _unitOfWork.GetFeedbackRepository().GetAllAsync();
                var feedbackResponses = feedbacks.Where(a=>!a.IsDeleted).Select(f => new FeedbackResponse
                {
                    FeedbackId = f.Id,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    BookingId = f.BookingId,
                    RenterId = f.RenterId
                }).ToList();
                return ResultResponse<List<FeedbackResponse>>.SuccessResult("Feedbacks retrieved successfully", feedbackResponses);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<FeedbackResponse>>.ServerError($"An error occurred while retrieving feedbacks: {ex.Message}");
            }
        }
        public async Task<ResultResponse<FeedbackResponse>> CreateFeedback(FeedbackRequest feedbackRequest)
        {
            try
            {
                var renterId=_currentUserService.UserId;
                var feedback = new Domain.Entities.Feedback
                {
                    Rating = feedbackRequest.Rating,
                    Comment = feedbackRequest.Comment,
                    BookingId = feedbackRequest.BookingId,
                    RenterId = Guid.Parse(renterId)
                };
                await _unitOfWork.GetFeedbackRepository().AddAsync(feedback);
                await _unitOfWork.SaveChangesAsync();
                return ResultResponse<FeedbackResponse>.SuccessResult("Feedback created successfully", new FeedbackResponse
                {
                    FeedbackId = feedback.Id,
                    Rating = feedback.Rating,
                    Comment = feedback.Comment,
                    BookingId = feedback.BookingId,
                    RenterId = feedback.RenterId
                });
            }
            catch (Exception ex)
            {
                return ResultResponse<FeedbackResponse>.ServerError($"An error occurred while creating feedback: {ex.Message}");
            }
        }
    }
}
