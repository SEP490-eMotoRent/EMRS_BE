using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.InsurancePackageDTOs;
using EMRS.Application.DTOs.RentalContractDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
            var availableVehicle = await _unitOfWork.GetVehicleRepository().GetOneRandomVehicleAsync(bookingCreateRequest.VehicleModelId);
            if (availableVehicle==null)
            {
                return ResultResponse<BookingResponse>.Failure("There are no available vehicle left at this branch.");
            }
            availableVehicle.Status=VehicleStatusEnum.Booked.ToString();

            var newBooking = new Booking
            {
                Id=Guid.NewGuid(),
                VehicleModelId = bookingCreateRequest.VehicleModelId,
                BookingStatus = BookingStatusEnum.Booked.ToString(),
                BaseRentalFee = bookingCreateRequest.BaseRentalFee,
                DepositAmount = bookingCreateRequest.DepositAmount,
                EndDatetime = bookingCreateRequest.EndDatetime,
                RenterId = userId,
                HandoverBranchId = bookingCreateRequest.HandoverBranchId,
                AverageRentalPrice = bookingCreateRequest.AverageRentalPrice,
                RentalDays = bookingCreateRequest.RentalDays,
                RentalHours = bookingCreateRequest.RentalHours,
                RentingRate = bookingCreateRequest.RentingRate,
                StartDatetime = bookingCreateRequest.StartDatetime,
                TotalRentalFee = bookingCreateRequest.TotalRentalFee,
                InsurancePackageId=bookingCreateRequest.InsurancePackageId
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
    public async Task<ResultResponse<List<BookingListForRenterResponse>>> GetAllBookingsByRenterIdAsync()
    {
        var currrentUser = _currentUserService.UserId;
        if (currrentUser != null)
        {

            var userId = Guid.Parse(_currentUserService.UserId);
            var bookings = await _unitOfWork.GetBookingRepository().GetBookingsByRenterIdAsync(userId);
            var bookingResponse = bookings.Select(a => new BookingListForRenterResponse
            {
                
                ActualReturnDatetime = a.ActualReturnDatetime,
                AverageRentalPrice = a.AverageRentalPrice,
                BaseRentalFee = a.BaseRentalFee,
                BookingStatus = a.BookingStatus,
                DepositAmount = a.DepositAmount,
                EndDatetime = a.EndDatetime,
                Id = a.Id,
                RenterId = a.RenterId,
                VehicleId = a.VehicleId,
                VehicleModelId = a.VehicleModelId,
                LateReturnFee = a.LateReturnFee,
                RentalDays = a.RentalDays,
                RentalHours = a.RentalHours,
                RentingRate = a.RentingRate,
                StartDatetime = a.StartDatetime,
                TotalAmount = a.TotalAmount,
                TotalRentalFee = a.TotalRentalFee,
                vehicleModel=a.VehicleModel==null?null : new VehicleModelResponse
                {
                    Id = a.Id,
                    BatteryCapacityKwh = a.VehicleModel.BatteryCapacityKwh,
                    Category = a.VehicleModel.Category,
                    Description = a.VehicleModel.Description,
                    MaxRangeKm = a.VehicleModel.MaxRangeKm,
                    MaxSpeedKmh = a.VehicleModel.MaxSpeedKmh,
                    ModelName = a.VehicleModel.ModelName,

                },
                renter=a.Renter==null?null: new RenterDetailResponse
                {
                    Id= a.Renter.Id,
                    Address = a.Renter.Address,
                    DateOfBirth = a.Renter.DateOfBirth,
                    Email = a.Renter.Email,
                    phone=a.Renter.phone,
                    account=new BookingDetailAccountResponse
                    {
                        Id=a.Renter.Account.Id,
                        Fullname=a.Renter.Account.Fullname,
                        Role=a.Renter.Account.Role,
                        Username=a.Renter.Account.Username,
                    }
                },
               insurancePackage=a.InsurancePackage==null?null: new InsurancePackageResponse
                {
                    Id = a.InsurancePackage.Id,
                    CoveragePersonLimit = a.InsurancePackage.CoveragePersonLimit,
                    CoveragePropertyLimit =a.InsurancePackage.CoveragePropertyLimit,
                    CoverageTheft=a.InsurancePackage.CoverageTheft,
                    CoverageVehiclePercentage=a.InsurancePackage.CoverageVehiclePercentage,
                    DeductibleAmount= a.InsurancePackage.DeductibleAmount,
                    Description= a.InsurancePackage.Description,
                    PackageFee= a.InsurancePackage.PackageFee,
                    PackageName = a.InsurancePackage.PackageName
                    
                }

            }).ToList();
            return ResultResponse<List<BookingListForRenterResponse>>.SuccessResult("Bookings retrieved successfully", bookingResponse);
        }
        else
        {
            return ResultResponse<List<BookingListForRenterResponse>>.NotFound("User not found");
        }
    }
    public async Task<ResultResponse<BookingResponse>> AssignVehicleForBookingIfBooked(Guid bookingId, Guid vehicleId)
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository().GetBookingByIdWithLessReferencesAsync(bookingId);
            var foundedVehicle= await _unitOfWork.GetVehicleRepository().GetVehicleWithReferences2Async(vehicleId);
            if (booking == null)
            {
                return ResultResponse<BookingResponse>.NotFound("Booking not found");
            }
            booking.VehicleId = vehicleId;
            foundedVehicle.Status = VehicleStatusEnum.Rented.ToString();
            _unitOfWork.GetBookingRepository().Update(booking);
            _unitOfWork.GetVehicleRepository().Update(foundedVehicle);
            await _unitOfWork.SaveChangesAsync();
            BookingResponse bookingResponse = _mapper.Map<BookingResponse>(booking);
            return ResultResponse<BookingResponse>.SuccessResult("Vehicle assigned successfully", bookingResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<BookingResponse>.Failure($"An error occurred while assigning vehicle: {ex.Message}");
        }
    }
    public async Task<ResultResponse<BookingResponse>> UpdateVehicleForBooking(Guid bookingId, Guid vehicleId)
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository().GetBookingByIdWithLessReferencesAsync(bookingId);
            var foundedVehicle = await _unitOfWork.GetVehicleRepository().GetVehicleWithReferences2Async(vehicleId);
            if (foundedVehicle.Status != VehicleStatusEnum.Available.ToString())
                return ResultResponse<BookingResponse>.Failure("Vehicle is not available for assignment");
            if (booking.VehicleId!=null)
            {
                var pastVehicle = await _unitOfWork.GetVehicleRepository().GetVehicleWithReferences2Async(booking.VehicleId.Value);
                if (pastVehicle != null)
                {
                    pastVehicle.Status = VehicleStatusEnum.Available.ToString();
                    _unitOfWork.GetVehicleRepository().Update(pastVehicle);
                }
            }    
            booking.VehicleId = vehicleId;
          
            
            booking.VehicleModelId = foundedVehicle.VehicleModel.Id;
            foundedVehicle.Status = VehicleStatusEnum.Rented.ToString();
            _unitOfWork.GetVehicleRepository().Update(foundedVehicle);
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
            var bookingList = bookings.Items.Select(b =>
            {

                var vehicle = b.Vehicle;
                var vehicleModel = b.VehicleModel;
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
                    VehicleModel= vehicleModel == null?null : new VehilceModelBookingResponse
                    {
                        Id = vehicleModel.Id,
                        BatteryCapacityKwh = vehicleModel.BatteryCapacityKwh,
                        Category = vehicleModel.Category,
                        MaxRangeKm = vehicleModel.MaxRangeKm,
                        MaxSpeedKmh = vehicleModel.MaxSpeedKmh,
                        ModelName = vehicleModel.ModelName
                    },
                     Vehicle=vehicle==null?null: new VehicleBookingResponse
                    {
                        RentalPricing = vehicle.VehicleModel?.RentalPricing?.RentalPrice ?? 0,
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
                        VehicleModel=new VehilceModelBookingResponse
                        {
                            Id = vehicle.VehicleModel.Id,
                            BatteryCapacityKwh = vehicle.VehicleModel.BatteryCapacityKwh,
                            Category = vehicle.VehicleModel.Category,
                            MaxRangeKm = vehicle.VehicleModel.MaxRangeKm,
                            MaxSpeedKmh = vehicle.VehicleModel.MaxSpeedKmh,
                            ModelName = vehicle.VehicleModel.ModelName
                        }
                    }
                  
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
    public async Task<ResultResponse<BookingDetailResponse>> GetBookingDetailAsync (Guid bookingId)
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository().GetBookingByIdWithLessReferencesAsync(bookingId);
            if (booking == null)
            {
                return ResultResponse<BookingDetailResponse>.NotFound("Booking not found");
            }
            var medias =  await _unitOfWork.GetMediaRepository().Query().ToListAsync();
            var rentalContractFile = booking.RentalContract == null
     ? null
     : medias
         .Where(a => a.EntityType == MediaEntityTypeEnum.RentalContract.ToString()
                     && a.DocNo == booking.RentalContract.Id)
         .Select(a => a.FileUrl)
         .FirstOrDefault();



            var vehicleFiles = booking.Vehicle == null
                ? new List<string>()
                : medias
                    .Where(a => a.EntityType == MediaEntityTypeEnum.Vehicle.ToString()
                                && a.DocNo == booking.Vehicle.Id)
                    .Select(a => a.FileUrl)
                    .ToList();
            string? checkListFile = null;
            var handoverFiles = new List<string>();

            if (booking.RentalReceipt != null)
            {
                foreach (var media in medias.Where(m => m.DocNo == booking.RentalReceipt.Id))
                {
                    switch (media.EntityType)
                    {
                        case nameof(MediaEntityTypeEnum.RentalReceiptCheckList):
                            checkListFile = media.FileUrl;
                            break;
                        case nameof(MediaEntityTypeEnum.RentalReceiptHandoverImage):
                            handoverFiles.Add(media.FileUrl);
                            break;
                    }
                }
            }

            BookingDetailResponse bookingResponse = new BookingDetailResponse
            {
                Id=booking.Id,
                BookingStatus=booking.BookingStatus,
                DepositAmount=booking.DepositAmount,
                EndDatetime=booking.EndDatetime,
                LateReturnFee=booking.LateReturnFee,
                RentalDays=booking.RentalDays,
                RentalHours=booking.RentalHours,
                RentingRate=booking.RentingRate,
                StartDatetime=booking.StartDatetime,
                TotalAmount=booking.TotalAmount,    
                TotalRentalFee=booking.TotalRentalFee,  
                BaseRentalFee = booking.BaseRentalFee,
                AverageRentalPrice = booking.AverageRentalPrice,
                ActualReturnDatetime = booking.ActualReturnDatetime,
                rentalContract=booking.RentalContract==null?null:new RentalContractResponse
                {
                    Id=booking.RentalContract.Id,
                    ContractStatus=booking.RentalContract.ContractStatus,
                    OtpCode=booking.RentalContract.OtpCode,
                    ExpireAt=booking.RentalContract.ExpireAt,
                    file= rentalContractFile 
                },
                vehicle=booking.Vehicle==null?null: new VehicleBookingDetailResponse
                {
                    Id= booking.Vehicle.Id,
                    Color= booking.Vehicle.Color,
                    CurrentOdometerKm=booking.Vehicle.CurrentOdometerKm,
                    BatteryHealthPercentage=booking.Vehicle.BatteryHealthPercentage,
                    LicensePlate=booking.Vehicle.LicensePlate,
                    NextMaintenanceDue=booking.Vehicle.NextMaintenanceDue,
                    Status=booking.Vehicle.Status,
                    FileUrl= vehicleFiles,
                    rentalPricing= new RentalPricingResponse
                    {
                        Id=booking.VehicleModel.RentalPricing.Id,
                        ExcessKmPrice=booking.VehicleModel.RentalPricing.ExcessKmPrice,
                        RentalPrice=booking.VehicleModel.RentalPricing.RentalPrice
                    },
                    vehicleModel= new VehicleModelResponse
                    {
                        BatteryCapacityKwh = booking.Vehicle.VehicleModel.BatteryCapacityKwh,
                        Category = booking.Vehicle.VehicleModel.Category,
                        Description = booking.Vehicle.VehicleModel.Description,
                        Id = booking.Vehicle.VehicleModel.Id,
                        MaxRangeKm = booking.Vehicle.VehicleModel.MaxRangeKm,
                        MaxSpeedKmh = booking.Vehicle.VehicleModel.MaxSpeedKmh,
                        ModelName = booking.Vehicle.VehicleModel.ModelName
                    }

                },
                vehicleModel =booking.VehicleModel==null?null:new VehicleModelResponse
                {
                    BatteryCapacityKwh=booking.VehicleModel.BatteryCapacityKwh,
                    Category=booking.VehicleModel.Category,
                    Description=booking.VehicleModel.Description,
                    Id=booking.VehicleModel.Id,
                    MaxRangeKm=booking.VehicleModel.MaxRangeKm,
                    MaxSpeedKmh= booking.VehicleModel.MaxSpeedKmh,
                    ModelName = booking.VehicleModel.ModelName
                },
                renter = booking.Renter == null ? null : new RenterDetailResponse
                {
                    Id=booking.Renter.Id,
                    Address=booking.Renter.Address,
                    DateOfBirth=booking.Renter.DateOfBirth,
                    Email=booking.Renter.Email,
                    phone=booking.Renter.phone,
                    account= new BookingDetailAccountResponse
                    {
                        Id=booking.Renter.AccountId,
                        Fullname= booking.Renter.Account.Fullname,
                        Role=booking.Renter.Account.Role,
                        Username= booking.Renter.Account.Username,
                    }
                },
                
                rentalReceipt= booking.RentalReceipt==null?null: new RentalReceiptResponse
                {
                    Id=booking.RentalReceipt.Id,
                    EndOdometerKm=booking.RentalReceipt.EndOdometerKm,
                    Notes=booking.RentalReceipt.Notes,
                    RenterConfirmedAt=booking.RentalReceipt.RenterConfirmedAt,
                    StartBatteryPercentage=booking.RentalReceipt.StartBatteryPercentage,
                    StartOdometerKm=booking.RentalReceipt.StartOdometerKm,
                    CheckListFile=checkListFile,
                    VehicleFiles= handoverFiles

                }
                
                
            };
            
            return ResultResponse<BookingDetailResponse>.SuccessResult("Booking status updated successfully", bookingResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<BookingDetailResponse>.Failure($"An error occurred  {ex.Message}");
        }
    }


}
