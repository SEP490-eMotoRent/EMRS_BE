using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
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

public class RentalReceiptService: IRentalReceiptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWalletService _walletService;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    public RentalReceiptService(ICloudinaryService cloudinaryService, IMapper mapper, IWalletService walletService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ResultResponse<RentalReceiptResponse>> CreateRentailReceiptAsync(RentalReceiptCreateRequest rentalReceiptCreateRequest)
    {
        try
        {
            var userId = Guid.Parse(_currentUserService.UserId);
            var booking= await _unitOfWork.GetBookingRepository().FindByIdAsync(rentalReceiptCreateRequest.BookingId);
            booking.HandoverBranchId = rentalReceiptCreateRequest.HandOverBranchId;
            var rentalReceipt = new RentalReceipt
            {
                Id = Guid.NewGuid(),
                BookingId = rentalReceiptCreateRequest.BookingId,
                Notes = rentalReceiptCreateRequest.Notes,
                StaffId = userId,
                RenterConfirmedAt = rentalReceiptCreateRequest.RenterConfirmedAt,

            };
            
            var url = await _cloudinaryService.UploadImageFileAsync(
                rentalReceiptCreateRequest.CheckListFile,
                $"img_{PublicIdGenerator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                "Images"
                );
            var checklistmedia = new Media
            {
                FileUrl = url,
                DocNo = rentalReceipt.Id,
                EntityType = MediaEntityTypeEnum.RentalReceiptCheckList.ToString(),
                MediaType = MediaTypeEnum.Image.ToString(),
            };

            var uploadTasks = rentalReceiptCreateRequest.VehicleFiles.Select(async file =>
            {

                var url = await _cloudinaryService.UploadImageFileAsync(
                    file,
                    $"img_{PublicIdGenerator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                    MediaTypeEnum.Image.ToString()
                    );
                return new Media
                {
                    EntityType = MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString(),
                    FileUrl = url,
                    DocNo = rentalReceipt.Id,
                    MediaType = MediaTypeEnum.Image.ToString(),
                };
            }).ToList();
            List<Media> medias = (await Task.WhenAll(uploadTasks)).ToList();
            await _unitOfWork.GetRentalReceiptRepository().AddAsync(rentalReceipt);
            await _unitOfWork.GetMediaRepository().AddRangeAsync(medias);
            await _unitOfWork.GetMediaRepository().AddAsync(checklistmedia);
             _unitOfWork.GetBookingRepository().Update(booking);
            await _unitOfWork.SaveChangesAsync();
            var rentalReceiptResponse = new RentalReceiptResponse
            {
                BookingId = rentalReceipt.BookingId,
                Notes = rentalReceipt.Notes,
                RenterConfirmedAt = rentalReceipt.RenterConfirmedAt,
                StaffId = userId,
                VehicleFiles = uploadTasks.Select(file =>
                    file.Result.FileUrl).ToList(),
                CheckListFile = checklistmedia.FileUrl,
            };
            return ResultResponse<RentalReceiptResponse>.SuccessResult("Bookings retrieved successfully", rentalReceiptResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<RentalReceiptResponse>.Failure($"An error occurred while creating the rental receipt: {ex.Message}");
        }
    }
 }
