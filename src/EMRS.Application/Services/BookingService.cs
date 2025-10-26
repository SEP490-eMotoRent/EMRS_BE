using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class BookingService:IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWalletService _walletService;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    public BookingService(ICloudinaryService cloudinaryService,IMapper mapper,IWalletService walletService,ICurrentUserService currentUserService,IUnitOfWork unitOfWork)
    {
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;   
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }
    public async Task<ResultResponse<BookingResponse>> CreateBooking(BookingCreateRequest bookingCreateRequest)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var userId = Guid.Parse(_currentUserService.UserId);
            var walletUser = await _unitOfWork.GetWalletRepository().GetWalletByAccountIdAsync(userId);
            if (walletUser.Balance < bookingCreateRequest.DepositAmount)
            {
                return ResultResponse<BookingResponse>.Failure("Insufficient balance in wallet.");
            }

            var newBooking = new Booking
            {
                Id=Guid.NewGuid(),
                VehicleModelId = bookingCreateRequest.VehicleModelId,
                BookingStatus = BookingStatusEnum.Booked.ToString(),
                BaseRentalFee = bookingCreateRequest.BaseRentalFee,
                DepositAmount = bookingCreateRequest.DepositAmount,
                EndDatetime = bookingCreateRequest.EndDatetime,
                RenterId = userId,
                AverageRentalPrice = bookingCreateRequest.AverageRentalPrice,
                RentalDays = bookingCreateRequest.RentalDays,
                RentalHours = bookingCreateRequest.RentalHours,
                RentingRate = bookingCreateRequest.RentingRate,
                StartDatetime = bookingCreateRequest.StartDatetime,
                TotalRentalFee = bookingCreateRequest.TotalRentalFee,
            };
            Transaction transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Status = TransactionStatusEnum.Success.ToString(),
                Amount = bookingCreateRequest.DepositAmount,
                TransactionType = TransactionTypeEnum.MakeDepositForBooking.ToString(),
                DocNo = newBooking.Id,
                CreatedAt = DateTime.UtcNow
                
            };
            await _unitOfWork.GetTransactionRepository().AddAsync(transaction);
            walletUser.Balance -= bookingCreateRequest.DepositAmount;
             _unitOfWork.GetWalletRepository().Update(walletUser);
            await _unitOfWork.GetBookingRepository().AddAsync(newBooking);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            BookingResponse bookingResponse = _mapper.Map<BookingResponse>(newBooking);
            return ResultResponse<BookingResponse>.SuccessResult("Booking created successfully", bookingResponse);

        }
        catch (Exception ex)
        {
            return ResultResponse<BookingResponse>.Failure($"An error occurred while creating the booking: {ex.Message}");
        }
    }
    public async Task<ResultResponse<List<BookingResponse>>> GetAllBookingsByRenterIdAsync()
    {
        var currrentUser = _currentUserService.UserId;
        if (currrentUser != null)
        {

            var userId = Guid.Parse(_currentUserService.UserId);
            var bookings = await _unitOfWork.GetBookingRepository().GetBookingsByRenterIdAsync(userId);
            List<BookingResponse> bookingResponses = _mapper.Map<List<BookingResponse>>(bookings);
            return ResultResponse<List<BookingResponse>>.SuccessResult("Bookings retrieved successfully", bookingResponses);
        }
        else
        {
            return ResultResponse<List<BookingResponse>>.NotFound("User not found");
        }
    }
    public async Task<ResultResponse<List<BookingResponse>>> GetAllBookings()
    {
        try
        {
            var bookings = await _unitOfWork.GetBookingRepository().GetAllAsync();
            List<BookingResponse> bookingResponses = _mapper.Map<List<BookingResponse>>(bookings);
            return ResultResponse<List<BookingResponse>>.SuccessResult("Bookings retrieved successfully", bookingResponses);

        }
        catch (Exception ex)
        {
            return ResultResponse<List<BookingResponse>>.Failure($"An error occurred while fetching the bookings: {ex.Message}");
        }
    }

    
    
}
