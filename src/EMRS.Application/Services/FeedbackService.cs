using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.FeedbackDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
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
        public async Task<ResultResponse<List<FeedbackDetailResponse>>> GetAllFeedbacks()
        {
            try
            {
                var feedbacks = (await _unitOfWork.GetFeedbackRepository().GetFeedbacksAsync()).Where(v=>!v.IsDeleted).ToList();
                if (feedbacks == null || !feedbacks.Any())
                {
                    return ResultResponse<List<FeedbackDetailResponse>>
                        .NotFound("No feedbacks found for the specified vehicle model.");
                }
                var feedbackResponses = feedbacks.Where(a=>!a.IsDeleted).Select(f => new FeedbackDetailResponse
                {
                    FeedbackId = f.Id,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    BookingId = f.BookingId,
                    RenterName = f.Renter.Account.Fullname
                }).ToList();
                return ResultResponse<List<FeedbackDetailResponse>>.SuccessResult("Feedbacks retrieved successfully", feedbackResponses);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<FeedbackDetailResponse>>.ServerError($"An error occurred while retrieving feedbacks: {ex.Message}");
            }
        }
        public async Task<ResultResponse<List<FeedbackDetailResponse>>> GetFeedbackByBookingId(Guid bookingId)
        {
            try
            {
                var feedbacks = await _unitOfWork
                    .GetFeedbackRepository()
                    .GetFeedbackByBookingIdAsync(bookingId);
                if (feedbacks == null || !feedbacks.Any())
                {
                    return ResultResponse<List<FeedbackDetailResponse>>
                        .NotFound("No feedbacks found for the specified vehicle model.");
                }
                var responses = feedbacks.Where(a => !a.IsDeleted).Select(f => new FeedbackDetailResponse
                {
                    FeedbackId = f.Id,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    BookingId = f.BookingId,
                    RenterName = f.Renter.Account.Fullname
                }).ToList();

                return ResultResponse<List<FeedbackDetailResponse>>
                    .SuccessResult("Feedbacks retrieved successfully", responses);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<FeedbackDetailResponse>>
                    .ServerError($"An error occurred while retrieving feedback: {ex.Message}");
            }
        }
        public async Task<ResultResponse<List<FeedbackDetailResponse>>> GetFeedbackByVehicleModelId(Guid vehicleModelId)
        {
            try
            {
                var feedbacks = await _unitOfWork
                    .GetFeedbackRepository()
                    .GetFeedbackByVehicleModelIdAsync(vehicleModelId);
                if(feedbacks == null || !feedbacks.Any())
                {
                    return ResultResponse<List<FeedbackDetailResponse>>
                        .NotFound("No feedbacks found for the specified vehicle model.");
                }
                var responses = feedbacks
                    .Where(a => !a.IsDeleted).Select(f => new FeedbackDetailResponse
                    {
                        FeedbackId = f.Id,
                        Rating = f.Rating,
                        Comment = f.Comment,
                        BookingId = f.BookingId,
                        RenterName = f.Renter.Account.Fullname
                    }).ToList();

                return ResultResponse<List<FeedbackDetailResponse>>
                    .SuccessResult("Feedbacks retrieved successfully", responses);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<FeedbackDetailResponse>>
                    .ServerError($"An error occurred while retrieving feedback: {ex.Message}");
            }
        }

        public async Task<ResultResponse<FeedbackResponse>> CreateFeedback(FeedbackRequest feedbackRequest)
        {
            try
            {
                var renterId=_currentUserService.UserId;
                var feedback = new Feedback
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
