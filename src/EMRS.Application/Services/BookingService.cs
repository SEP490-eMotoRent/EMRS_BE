using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
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
    public async Task<ResultResponse<BookingResponse>> AssignVehicleForBooking(Guid bookingId, Guid vehicleId)
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository().FindByIdAsync(bookingId);
            if (booking == null)
            {
                return ResultResponse<BookingResponse>.NotFound("Booking not found");
            }
            booking.VehicleId = vehicleId;
         
            _unitOfWork.GetBookingRepository().Update(booking);
            await _unitOfWork.SaveChangesAsync();
            BookingResponse bookingResponse = _mapper.Map<BookingResponse>(booking);
            return ResultResponse<BookingResponse>.SuccessResult("Vehicle assigned successfully", bookingResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<BookingResponse>.Failure($"An error occurred while assigning vehicle: {ex.Message}");
        }
    }
    public async Task<ResultResponse<PaginationResult<List<BookingForStaffResponse>>>> GetAllBookings(BookingSearchRequest bookingSearchRequest,int PageNum,int PageSize)
    {
        try
        {
           
            var bookings = await _unitOfWork.GetBookingRepository().GetBookingWithFilter(bookingSearchRequest,PageSize,PageNum);

            var medias = await _unitOfWork.GetMediaRepository().Query().Where(a =>
                 a.EntityType == MediaEntityTypeEnum.Vehicle.ToString()).ToListAsync();
            var mediaDict = medias
             .GroupBy(a => a.DocNo)
             .ToDictionary(g => g.Key, g => g.ToList());
            var bookingList=bookings.Items.Select(b =>
            {
             
                var vehicle = b.Vehicle; 
                
                VehicleBookingResponse? vehicleResponse = null;
                if (vehicle != null)
                {
                    vehicleResponse = new VehicleBookingResponse
                    {
                        RentalPricing=vehicle?.VehicleModel?.RentalPricing?.RentalPrice??0,

                        Id = vehicle.Id,
                        Color = vehicle.Color,
                        CurrentOdometerKm = vehicle.CurrentOdometerKm,
                        BatteryHealthPercentage = vehicle.BatteryHealthPercentage,
                        Status = vehicle.Status,
                        LicensePlate = vehicle.LicensePlate,
                        NextMaintenanceDue = vehicle.NextMaintenanceDue,
                        FileUrl = mediaDict.TryGetValue(vehicle.Id, out var mediaVehicleList)
                            ? mediaVehicleList.Select(m => m.FileUrl).ToList()
                            : new List<string>(),
                        VehicleModel = new VehilceModelBookingResponse
                        {
                            Id = vehicle.VehicleModel.Id,
                           BatteryCapacityKwh = vehicle.VehicleModel.BatteryCapacityKwh,
                            Category = vehicle.VehicleModel.Category,
                            MaxRangeKm = vehicle.VehicleModel.MaxRangeKm,
                            MaxSpeedKmh = vehicle.VehicleModel.MaxSpeedKmh,
                            ModelName = vehicle.VehicleModel.ModelName
                            
                        }
                    };
                }
                
                return new BookingForStaffResponse
                {
                    Id = b.Id,
                    BookingStatus = b.BookingStatus,
                    BaseRentalFee = b.BaseRentalFee,
                    DepositAmount = b.DepositAmount,
                    EndDatetime = b.EndDatetime,
                    AverageRentalPrice = b.AverageRentalPrice,
                    RentalDays = b.RentalDays,
                    RentalHours = b.RentalHours,
                    RentingRate = b.RentingRate,
                    StartDatetime = b.StartDatetime,
                    TotalRentalFee = b.TotalRentalFee,
                    ActualReturnDatetime = b.ActualReturnDatetime,
                    LateReturnFee = b.LateReturnFee,
                    TotalAmount = b.TotalAmount,
                    Renter = new RenterBookingResponse
                    {
                        Id = b.Renter.Id,
                       
                        Email = b.Renter.Email,
                        phone = b.Renter.phone,
                        Address = b.Renter.Address,
                        Account = new AccountBookingResponse
                        {
                            Id = b.Renter.Account.Id,
                            Username = b.Renter.Account.Username,
                            Role = b.Renter.Account.Role,
                            Fullname = b.Renter.Account.Fullname
                        }
                    },
                    Vehicle= vehicleResponse
                };
            }).ToList();
            var response= new PaginationResult<List<BookingForStaffResponse>>
            {
                PageSize = bookings.PageSize,
                CurrentPage = bookings.CurrentPage,
                TotalItems = bookings.TotalItems,
                TotalPages = bookings.TotalPages,
                Items = bookingList
            };
            return ResultResponse<PaginationResult<List<BookingForStaffResponse>>>.SuccessResult("Bookings retrieved successfully", response);

        }
        catch (Exception ex)
        {
            return ResultResponse<PaginationResult<List<BookingForStaffResponse>>>.Failure($"An error occurred while fetching the bookings: {ex.Message}");
        }
    }

    
    
}
