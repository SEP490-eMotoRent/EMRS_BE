using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.BackgroundJobs.Booking;
using EMRS.Application.Abstractions.Models.VNPay;
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
using Microsoft.AspNetCore.Http;
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
    private readonly IVNPayService _vnPayService;
    private readonly IBookingJobScheduler _bookingJobScheduler;
    public BookingService(IBookingJobScheduler bookingJobScheduler,IVNPayService vNPayService,ICloudinaryService cloudinaryService,IMapper mapper,IWalletService walletService,ICurrentUserService currentUserService,IUnitOfWork unitOfWork)
    {
        _bookingJobScheduler = bookingJobScheduler;
        _vnPayService = vNPayService;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;   
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ResultResponse<bool>> ProcessCallBack(VNPayResponseData vNPayResponseData)
    {
        try
        {
            var response = vNPayResponseData;
            if (!response.IsSuccess)
                return ResultResponse<bool>.Failure(response.Message);
            var booking = _unitOfWork.GetBookingRepository()
                .GetAll().FirstOrDefault(b => b.BookingCode == response.OrderId);
            if (booking == null)
                return ResultResponse<bool>.Failure("Booking not found");

            var vehicle =  _unitOfWork.GetVehicleRepository().GetAll()
               
                 .OrderBy(v => Guid.NewGuid()).FirstOrDefault
                (b => b.Status == VehicleStatusEnum.Hold.ToString()&&b.VehicleModelId==booking.VehicleModelId);
            if (vehicle != null)
            {
                vehicle.Status = response.ResponseCode == "00"
                    ? VehicleStatusEnum.Booked.ToString()
                    : VehicleStatusEnum.Available.ToString();

                _unitOfWork.GetVehicleRepository().Update(vehicle);
            }
            Transaction transaction;
            if (response.ResponseCode == "00")
            {

               
                booking.BookingStatus = BookingStatusEnum.Booked.ToString();
                transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Status = TransactionStatusEnum.Success.ToString(),
                    Amount = booking.DepositAmount,
                    TransactionType = TransactionTypeEnum.MakeDepositForBooking.ToString(),
                    DocNo = booking.Id,
                    CreatedAt = DateTime.UtcNow

                };
                 
                 _unitOfWork.GetBookingRepository().Update(booking);
                return ResultResponse<bool>.SuccessResult("Payment success", true);
            }
             transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Status = TransactionStatusEnum.Failed.ToString(),
                Amount = booking.DepositAmount,
                TransactionType = TransactionTypeEnum.BookingDeposit.ToString(),
                DocNo = booking.Id,
                CreatedAt = DateTime.UtcNow

            };
            await _unitOfWork.GetTransactionRepository().AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
            return ResultResponse<bool>.Failure($"Payment failed: {response.ResponseCode}");
        }
        catch (Exception ex)
        {
            return ResultResponse<bool>.Failure($"VNPay IPN error: {ex.Message}");
        }
    }
    public async Task<ResultResponse<BookingResponse>> CancelBookingByCustomerAsync(Guid bookingId)
    {
        try
        {
            var userId= Guid.Parse(_currentUserService.UserId);
            var booking = await _unitOfWork.GetBookingRepository().GetBoookingForUpdatingAsync(bookingId);
            if (booking == null)
            {
                return ResultResponse<BookingResponse>.NotFound("Booking not found");
            }
            var userwallet = await _unitOfWork.GetWalletRepository().GetWalletByRenterIdForModifyAsync(userId);
            var refundFee=  await _unitOfWork.GetConfigurationRepository().Query().FirstOrDefaultAsync(a=>a.Type==(int)ConfigurationTypeEnum.RefundRate);
            if(refundFee == null)
            {
                return ResultResponse<BookingResponse>.Failure("Refund rate configuration not found");
            }    
            var refundAmount = booking.DepositAmount;

            var vietnamNow = DateTimeHelper.ToVietnamTime(DateTimeOffset.UtcNow);
            var vietnamCreated = DateTimeHelper.ToVietnamTime(booking.CreatedAt);

            if (vietnamCreated.HasValue && vietnamCreated.Value.AddHours(24) < vietnamNow)
            {
                return ResultResponse<BookingResponse>.Failure("You can only cancel booking within 24 hours of creation time.");
            }

            if (!decimal.TryParse(refundFee?.Value, out var refundRate))
                refundRate = 0;
            refundAmount = refundAmount * refundRate;
            
            booking.BookingStatus = BookingStatusEnum.Cancelled.ToString();
            userwallet.Balance += refundAmount;
            _unitOfWork.GetBookingRepository().Update(booking);
            _unitOfWork.GetWalletRepository().Update(userwallet);
            await _unitOfWork.SaveChangesAsync();
            var response = new BookingResponse
            {
                ActualReturnDatetime = booking.ActualReturnDatetime.HasValue
         ? DateTimeHelper.ToVietnamTime(booking.ActualReturnDatetime.Value)
         : null,
                AverageRentalPrice = booking.AverageRentalPrice,
                BaseRentalFee = booking.BaseRentalFee,
                BookingCode = booking.BookingCode,
                BookingStatus = booking.BookingStatus,
                DepositAmount = booking.DepositAmount,
                EndDatetime = booking.EndDatetime.HasValue
         ? DateTimeHelper.ToVietnamTime(booking.EndDatetime.Value)
         : null,
                Id = booking.Id,
                RenterId = booking.RenterId,
                LateReturnFee = booking.LateReturnFee,
                RentalDays = booking.RentalDays,
                RentalHours = booking.RentalHours,
                RentingRate = booking.RentingRate,
                StartDatetime = booking.StartDatetime.HasValue
         ? DateTimeHelper.ToVietnamTime(booking.StartDatetime.Value)
         : null,
                TotalAmount = booking.TotalAmount,
                TotalRentalFee = booking.TotalRentalFee,
                VehicleModelId = booking.VehicleModelId,
                VehicleId = booking.VehicleId
            };

            return ResultResponse<BookingResponse>.SuccessResult("Booking cancelled successfully", response);
        }
        catch(Exception ex)
        {
            return ResultResponse<BookingResponse>.Failure($"An error occurred while cancelling the booking: {ex.Message}");
        }
    }

    public async Task<ResultResponse<BookingResponse>> CreateBooking(BookingCreateRequest bookingCreateRequest)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            decimal totalAmount = bookingCreateRequest.DepositAmount;

            if (bookingCreateRequest.InsurancePackageId != null)
            {
                var insurance = await _unitOfWork.GetInsurancePackageRepository()
                    .FindByIdAsync(bookingCreateRequest.InsurancePackageId.Value);

                if (insurance != null)
                {
                    totalAmount += insurance.PackageFee;
                }
            }
            var userId = Guid.Parse(_currentUserService.UserId);
            if(await _unitOfWork.GetDocumentRepository().HasBothDocumentImagesAsync(userId)==false)
            {
                return ResultResponse<BookingResponse>.Failure("You must upload your identification and driving documents before making a booking.");
            }
            var walletUser = await _unitOfWork.GetWalletRepository().GetWalletByRenterIdForModifyAsync(userId);
            if (walletUser.Balance < totalAmount)
            {
                return ResultResponse<BookingResponse>.Failure("Insufficient balance in wallet.");
            }
            var availableVehicle = await _unitOfWork.GetVehicleRepository().GetOneRandomVehicleAsync(bookingCreateRequest.VehicleModelId);
            if (availableVehicle == null)
            {
                return ResultResponse<BookingResponse>.Failure("There are no available vehicle left at this branch.");
            }
            availableVehicle.Status = VehicleStatusEnum.Booked.ToString();
          
            var newBooking = new Booking
            {
                Id = Guid.NewGuid(),
                VehicleModelId = bookingCreateRequest.VehicleModelId,
                BookingStatus = BookingStatusEnum.Booked.ToString(),
                BaseRentalFee = bookingCreateRequest.BaseRentalFee,
                DepositAmount = bookingCreateRequest.DepositAmount,
                EndDatetime = bookingCreateRequest.EndDatetime,
                BookingCode= Generator.BookingCodeGenerate(),
                RenterId = userId,
                HandoverBranchId = bookingCreateRequest.HandoverBranchId,
                AverageRentalPrice = bookingCreateRequest.AverageRentalPrice,
                RentalDays = bookingCreateRequest.RentalDays,
                RentalHours = bookingCreateRequest.RentalHours,
                RentingRate = bookingCreateRequest.RentingRate,
                StartDatetime = bookingCreateRequest.StartDatetime,
                TotalRentalFee = bookingCreateRequest.TotalRentalFee,
                InsurancePackageId = bookingCreateRequest.InsurancePackageId != null
    ? bookingCreateRequest.InsurancePackageId
    : null
            };
            Transaction transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Status = TransactionStatusEnum.Success.ToString(),
                Amount = totalAmount,
                TransactionType = TransactionTypeEnum.BookingDeposit.ToString(),
                DocNo = newBooking.Id,
                CreatedAt = DateTime.UtcNow

            };

            await _unitOfWork.GetTransactionRepository().AddAsync(transaction);
            walletUser.Balance -= totalAmount;
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
                ActualReturnDatetime = a.ActualReturnDatetime.HasValue ? DateTimeHelper.ToVietnamTime(a.ActualReturnDatetime.Value) : null,
                StartDatetime = a.StartDatetime.HasValue ? DateTimeHelper.ToVietnamTime(a.StartDatetime.Value) : null,
                EndDatetime = a.EndDatetime.HasValue ? DateTimeHelper.ToVietnamTime(a.EndDatetime.Value) : null,

                AverageRentalPrice = a.AverageRentalPrice,
                BaseRentalFee = a.BaseRentalFee,
                BookingStatus = a.BookingStatus,
                DepositAmount = a.DepositAmount,
                Id = a.Id,
                RenterId = a.RenterId,
                VehicleId = a.VehicleId,
                VehicleModelId = a.VehicleModelId,
                LateReturnFee = a.LateReturnFee,
                RentalDays = a.RentalDays,
                RentalHours = a.RentalHours,
                RentingRate = a.RentingRate,
                TotalAmount = a.TotalAmount,
                TotalRentalFee = a.TotalRentalFee,

                vehicleModel = a.VehicleModel == null ? null : new VehicleModelResponse
                {
                    Id = a.VehicleModel.Id,
                    BatteryCapacityKwh = a.VehicleModel.BatteryCapacityKwh,
                    Category = a.VehicleModel.Category,
                    Description = a.VehicleModel.Description,
                    MaxRangeKm = a.VehicleModel.MaxRangeKm,
                    MaxSpeedKmh = a.VehicleModel.MaxSpeedKmh,
                    ModelName = a.VehicleModel.ModelName,
                },

                renter = a.Renter == null ? null : new RenterDetailResponse
                {
                    Id = a.Renter.Id,
                    Address = a.Renter.Address,
                    DateOfBirth = a.Renter.DateOfBirth,
                    Email = a.Renter.Email,
                    phone = a.Renter.phone,
                    account = a.Renter.Account == null ? null : new BookingDetailAccountResponse
                    {
                        Id = a.Renter.Account.Id,
                        Fullname = a.Renter.Account.Fullname,
                        Role = a.Renter.Account.Role,
                        Username = a.Renter.Account.Username,
                    }
                },

                insurancePackage = a.InsurancePackage == null ? null : new InsurancePackageResponse
                {
                    Id = a.InsurancePackage.Id,
                    CoveragePersonLimit = a.InsurancePackage.CoveragePersonLimit,
                    CoveragePropertyLimit = a.InsurancePackage.CoveragePropertyLimit,
                    CoverageTheft = a.InsurancePackage.CoverageTheft,
                    CoverageVehiclePercentage = a.InsurancePackage.CoverageVehiclePercentage,
                    DeductibleAmount = a.InsurancePackage.DeductibleAmount,
                    Description = a.InsurancePackage.Description,
                    PackageFee = a.InsurancePackage.PackageFee,
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
            var booking = await _unitOfWork.GetBookingRepository().FindByIdAsync(bookingId);
            var foundedVehicle= await _unitOfWork.GetVehicleRepository().FindByIdAsync(vehicleId);
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
                    EndDatetime = b.EndDatetime.HasValue ? DateTimeHelper.ToVietnamTime(b.EndDatetime.Value) : null,
                    AverageRentalPrice = b.AverageRentalPrice,
                    RentalDays = b.RentalDays,
                    RentalHours = b.RentalHours,
                    RentingRate = b.RentingRate,
                    StartDatetime = b.StartDatetime.HasValue ? DateTimeHelper.ToVietnamTime(b.StartDatetime.Value) : null,
                    TotalRentalFee = b.TotalRentalFee,
                    ActualReturnDatetime = b.ActualReturnDatetime.HasValue ? DateTimeHelper.ToVietnamTime(b.ActualReturnDatetime.Value) : null,
                    LateReturnFee = b.LateReturnFee,
                    TotalAmount = b.TotalAmount,

                    Renter = b.Renter == null ? null : new RenterBookingResponse
                    {
                        Id = b.Renter.Id,
                        Email = b.Renter.Email,
                        phone = b.Renter.phone,
                        Address = b.Renter.Address,
                        Account = b.Renter.Account == null ? null : new AccountBookingResponse
                        {
                            Id = b.Renter.Account.Id,
                            Username = b.Renter.Account.Username,
                            Role = b.Renter.Account.Role,
                            Fullname = b.Renter.Account.Fullname
                        }
                    },

                    VehicleModel = vehicleModel == null ? null : new VehilceModelBookingResponse
                    {
                        Id = vehicleModel.Id,
                        BatteryCapacityKwh = vehicleModel.BatteryCapacityKwh,
                        Category = vehicleModel.Category,
                        MaxRangeKm = vehicleModel.MaxRangeKm,
                        MaxSpeedKmh = vehicleModel.MaxSpeedKmh,
                        ModelName = vehicleModel.ModelName
                    },

                    Vehicle = vehicle == null ? null : new VehicleBookingResponse
                    {
                        RentalPricing = vehicle.VehicleModel?.RentalPricing?.RentalPrice ?? 0,
                        Id = vehicle.Id,
                        Color = vehicle.Color,
                        CurrentOdometerKm = vehicle.CurrentOdometerKm,
                        BatteryHealthPercentage = vehicle.BatteryHealthPercentage,
                        Status = vehicle.Status,
                        LicensePlate = vehicle.LicensePlate,
                        NextMaintenanceDue = vehicle.NextMaintenanceDue.HasValue ? DateTimeHelper.ToVietnamTime(vehicle.NextMaintenanceDue.Value) : null,
                        FileUrl = mediaDict.TryGetValue(vehicle.Id, out var mediaVehicleList)
                            ? mediaVehicleList.Select(m => m.FileUrl).ToList()
                            : new List<string>(),
                        VehicleModel = vehicle.VehicleModel == null ? null : new VehilceModelBookingResponse
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
    public async Task<ResultResponse<BookingDetailResponse>> GetBookingDetailAsync(Guid bookingId)
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingByIdWithLessReferencesAsync(bookingId);

            if (booking == null)
                return ResultResponse<BookingDetailResponse>.NotFound("Booking not found");

            var medias = await _unitOfWork.GetMediaRepository().Query().ToListAsync();

            var vehicleFiles = new List<string>();
            var rentalContractFile = (string?)null;
            var allCheckListFiles = new List<string>();
            var allHandoverFiles = new List<string>();
            var allReturnFiles = new List<string>();
            var rentalReceipts = new List<RentalReceiptResponse>();

            if (booking.RentalContract != null)
            {
                rentalContractFile = medias
                    .Where(a => a.EntityType == MediaEntityTypeEnum.RentalContract.ToString()
                                && a.DocNo == booking.RentalContract.Id)
                    .Select(a => a.FileUrl)
                    .FirstOrDefault();
            }

            if (booking.Vehicle != null)
            {
                vehicleFiles = medias
                    .Where(a => a.EntityType == MediaEntityTypeEnum.Vehicle.ToString()
                                && a.DocNo == booking.Vehicle.Id)
                    .Select(a => a.FileUrl)
                    .ToList();
            }

            if (booking.RentalReceipts != null && booking.RentalReceipts.Any())
            {
                foreach (var receipt in booking.RentalReceipts)
                {
                    var checkListFile = new List<string>();
                    var handoverFiles = new List<string>();
                    var returnFiles = new List<string>();

                    var relatedMedias = medias.Where(m => m.DocNo == receipt.Id).ToList();

                    foreach (var media in relatedMedias)
                    {
                        switch (media.EntityType)
                        {
                            case nameof(MediaEntityTypeEnum.RentalReceiptCheckListHandOver):
                                checkListFile.Add(media.FileUrl);
                                allCheckListFiles.Add(media.FileUrl);
                                break;
                            case nameof(MediaEntityTypeEnum.RentalReceiptHandoverImage):
                                handoverFiles.Add(media.FileUrl);
                                allHandoverFiles.Add(media.FileUrl);
                                break;
                            case nameof(MediaEntityTypeEnum.RentalReceiptReturnImage):
                                returnFiles.Add(media.FileUrl);
                                allReturnFiles.Add(media.FileUrl);
                                break;
                        }
                    }

                    rentalReceipts.Add(new RentalReceiptResponse
                    {
                        Id = receipt.Id,
                        EndOdometerKm = receipt.EndOdometerKm,
                        Notes = receipt.Notes,
                        RenterConfirmedAt = receipt.RenterConfirmedAt,
                        StartBatteryPercentage = receipt.StartBatteryPercentage,
                        StartOdometerKm = receipt.StartOdometerKm,
                        CheckListFile = checkListFile,
                        HandOverVehicleImageFiles = handoverFiles,
                        ReturnVehicleImageFiles = returnFiles
                    });
                }
            }

            var bookingResponse = new BookingDetailResponse
            {
                Id = booking.Id,
                BookingStatus = booking.BookingStatus,
                DepositAmount = booking.DepositAmount,
                EndDatetime = booking.EndDatetime.HasValue
        ? DateTimeHelper.ToVietnamTime(booking.EndDatetime.Value)
        : (DateTime?)null,
                LateReturnFee = booking.LateReturnFee,
                RentalDays = booking.RentalDays,
                RentalHours = booking.RentalHours,
                RentingRate = booking.RentingRate,
                StartDatetime = DateTimeHelper.ToVietnamTime(booking.StartDatetime),
                TotalAmount = booking.TotalAmount,
                TotalRentalFee = booking.TotalRentalFee,
                BaseRentalFee = booking.BaseRentalFee,
                AverageRentalPrice = booking.AverageRentalPrice,
                ActualReturnDatetime = booking.ActualReturnDatetime.HasValue
        ? DateTimeHelper.ToVietnamTime(booking.ActualReturnDatetime.Value)
        : (DateTime?)null,

                rentalContract = booking.RentalContract == null ? null : new RentalContractResponse
                {
                    Id = booking.RentalContract.Id,
                    ContractStatus = booking.RentalContract.ContractStatus,
                    OtpCode = booking.RentalContract.OtpCode,
                    ExpireAt = booking.RentalContract.ExpireAt.HasValue
            ? DateTimeHelper.ToVietnamTime(booking.RentalContract.ExpireAt.Value)
            : (DateTime?)null,
                    file = rentalContractFile
                },

                vehicle = booking.Vehicle == null ? null : new VehicleBookingDetailResponse
                {
                    Id = booking.Vehicle.Id,
                    Color = booking.Vehicle.Color,
                    CurrentOdometerKm = booking.Vehicle.CurrentOdometerKm,
                    BatteryHealthPercentage = booking.Vehicle.BatteryHealthPercentage,
                    LicensePlate = booking.Vehicle.LicensePlate,
                    NextMaintenanceDue = booking.Vehicle.NextMaintenanceDue.HasValue
            ? DateTimeHelper.ToVietnamTime(booking.Vehicle.NextMaintenanceDue.Value)
            : (DateTime?)null,
                    Status = booking.Vehicle.Status,
                    FileUrl = vehicleFiles,
                    rentalPricing = booking.VehicleModel?.RentalPricing == null ? null : new RentalPricingResponse
                    {
                        Id = booking.VehicleModel.RentalPricing.Id,
                        ExcessKmPrice = booking.VehicleModel.RentalPricing.ExcessKmPrice,
                        RentalPrice = booking.VehicleModel.RentalPricing.RentalPrice
                    },
                    vehicleModel = booking.Vehicle?.VehicleModel == null ? null : new VehicleModelResponse
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

                vehicleModel = booking.VehicleModel == null ? null : new VehicleModelResponse
                {
                    BatteryCapacityKwh = booking.VehicleModel.BatteryCapacityKwh,
                    Category = booking.VehicleModel.Category,
                    Description = booking.VehicleModel.Description,
                    Id = booking.VehicleModel.Id,
                    MaxRangeKm = booking.VehicleModel.MaxRangeKm,
                    MaxSpeedKmh = booking.VehicleModel.MaxSpeedKmh,
                    ModelName = booking.VehicleModel.ModelName
                },

                renter = booking.Renter == null ? null : new RenterDetailResponse
                {
                    Id = booking.Renter.Id,
                    Address = booking.Renter.Address,
                    DateOfBirth = booking.Renter.DateOfBirth,
                    Email = booking.Renter.Email,
                    phone = booking.Renter.phone,
                    account = new BookingDetailAccountResponse
                    {
                        Id = booking.Renter.AccountId,
                        Fullname = booking.Renter.Account.Fullname,
                        Role = booking.Renter.Account.Role,
                        Username = booking.Renter.Account.Username
                    }
                },

                rentalReceipt = rentalReceipts
            };

            return ResultResponse<BookingDetailResponse>.SuccessResult("Booking retrieved successfully", bookingResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<BookingDetailResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultResponse<BookingWithoutWalletResponse>> CreateBookingWithoutWallet(BookingCreateRequest bookingCreateRequest)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var userId = Guid.Parse(_currentUserService.UserId);
            if (await _unitOfWork.GetDocumentRepository().HasBothDocumentImagesAsync(userId) == false)
            {
                return ResultResponse<BookingWithoutWalletResponse>.Failure("You must upload your identification and driving documents before making a booking.");
            }
            var availableVehicle = await _unitOfWork.GetVehicleRepository().GetOneRandomVehicleAsync(bookingCreateRequest.VehicleModelId);
            if (availableVehicle == null)
            {
                return ResultResponse<BookingWithoutWalletResponse>.Failure("There are no available vehicle left at this branch.");
            }
            availableVehicle.Status = VehicleStatusEnum.Hold.ToString();


            var newBooking = new Booking
            {
                Id = Guid.NewGuid(),
                VehicleModelId = bookingCreateRequest.VehicleModelId,
                BookingStatus = BookingStatusEnum.Pending.ToString(),
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
                InsurancePackageId = bookingCreateRequest.InsurancePackageId != null
    ? bookingCreateRequest.InsurancePackageId
    : null,
                BookingCode = Generator.BookingCodeGenerate()
            };
            decimal totalAmount = bookingCreateRequest.DepositAmount;

            if (bookingCreateRequest.InsurancePackageId != null)
            {
                var insurance = await _unitOfWork.GetInsurancePackageRepository()
                    .FindByIdAsync(bookingCreateRequest.InsurancePackageId.Value);

                if (insurance != null)
                {
                    totalAmount += insurance.PackageFee;
                }
            }
            VNPayRequestData data = new VNPayRequestData
            {
                Amount = totalAmount,
                OrderDescription = BookingStatusEnum.Booked.ToString(),
                OrderId = newBooking.BookingCode
            };
            string? vnpayurl = _vnPayService.CreatePaymentUrl(data);

            await _unitOfWork.GetBookingRepository().AddAsync(newBooking);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            BookingWithoutWalletResponse response = new BookingWithoutWalletResponse
            {
                Id = newBooking.Id,
                ActualReturnDatetime = newBooking.ActualReturnDatetime.HasValue
        ? DateTimeHelper.ToVietnamTime(newBooking.ActualReturnDatetime.Value)
        : (DateTime?)null,
                AverageRentalPrice = newBooking.AverageRentalPrice,
                BaseRentalFee = newBooking.BaseRentalFee,
                BookingStatus = newBooking.BookingStatus,
                DepositAmount = newBooking.DepositAmount,
                EndDatetime = newBooking.EndDatetime.HasValue
        ? DateTimeHelper.ToVietnamTime(newBooking.EndDatetime.Value)
        : (DateTime?)null,
                LateReturnFee = newBooking.LateReturnFee,
                RentalDays = newBooking.RentalDays,
                RentalHours = newBooking.RentalHours,
                RenterId = newBooking.RenterId,
                RentingRate = newBooking.RentingRate,
                StartDatetime = DateTimeHelper.ToVietnamTime(newBooking.StartDatetime),
                TotalAmount = newBooking.TotalAmount,
                TotalRentalFee = newBooking.TotalRentalFee,
                VehicleId = newBooking.VehicleId,
                VehicleModelId = newBooking.VehicleModelId,
                VNPAYURL = vnpayurl
            };
            _bookingJobScheduler.ScheduleAutoCancel(newBooking.Id, TimeSpan.FromMinutes(15));
            return ResultResponse<BookingWithoutWalletResponse>.SuccessResult("Booking created successfully", response);
            

        }
        catch (Exception ex)
        {
            return ResultResponse<BookingWithoutWalletResponse>.Failure($"An error occurred while creating the booking: {ex.Message}");
        }
    }
   

}
