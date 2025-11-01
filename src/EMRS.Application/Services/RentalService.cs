using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.RentalContractDTOs;
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

public class RentalService: IRentalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWalletService _walletService;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IQuestPdfGenerator _pdfGenerator;
    public RentalService(IQuestPdfGenerator puppeteerPdfGenerator,IEmailService emailService,ICloudinaryService cloudinaryService, IMapper mapper, IWalletService walletService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _pdfGenerator = puppeteerPdfGenerator;
        _emailService = emailService;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }
    public async Task<ResultResponse<RentalReceiptResponse>> GetRentalReceiptDetailByBookingIdAsync(Guid bookingId)
    {
        try
        {
            var rentalReceipt = await _unitOfWork
                .GetRentalReceiptRepository()
                .GetRentalReceiptByBookingId(bookingId);

            if (rentalReceipt == null)
                return ResultResponse<RentalReceiptResponse>.NotFound("There are no rental receipts found");

            var medias = await _unitOfWork.GetMediaRepository().Query()
                .Where(a => a.DocNo == rentalReceipt.Id &&
                    (a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListHandOver.ToString() ||
                     a.EntityType == MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString() ||
                     a.EntityType == MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()))
                .ToListAsync();

            var response = new RentalReceiptResponse
            {
                Id = rentalReceipt.Id,
                StaffId = rentalReceipt.StaffId,
                StartBatteryPercentage = rentalReceipt.StartBatteryPercentage,
                StartOdometerKm = rentalReceipt.StartOdometerKm,
                EndOdometerKm = rentalReceipt.EndOdometerKm,
                BookingId = rentalReceipt.BookingId,
                Notes = rentalReceipt.Notes,
                RenterConfirmedAt = rentalReceipt.RenterConfirmedAt,
                HandOverVehicleImageFiles = new List<string>(),
                ReturnVehicleImageFiles = new List<string>(),
                CheckListFile= new List<string>(),
            };

            foreach (var media in medias)
            {
                switch (media.EntityType)
                {
                    case nameof(MediaEntityTypeEnum.RentalReceiptHandoverImage):
                        response.HandOverVehicleImageFiles.Add(media.FileUrl);
                        break;

                    case nameof(MediaEntityTypeEnum.RentalReceiptReturnImage):
                        response.ReturnVehicleImageFiles.Add(media.FileUrl);
                        break;

                    case nameof(MediaEntityTypeEnum.RentalReceiptCheckListHandOver):
                        response.CheckListFile.Add(media.FileUrl); 
                        break;
                }
            }

            return ResultResponse<RentalReceiptResponse>.SuccessResult("There is a rental receipt found", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<RentalReceiptResponse>.Failure(
                $"An error occurred while retrieving rental receipts: {ex.Message}"
            );
        }
    }

    public async Task<ResultResponse<string>> SendRenterCodeForOtpSignAsync(Guid rentalContractId)
    {
        try
        {
            var renter = await _unitOfWork.GetRenterRepository().FindByIdAsync(Guid.Parse(_currentUserService.UserId));
            if (renter == null)
                return ResultResponse<string>.Unauthorized("Please log in as renter");

            var rentalContract = await _unitOfWork.GetRentalContractRepository().FindByIdAsync(rentalContractId);
            if (rentalContract == null)
                return ResultResponse<string>.NotFound("There are no rental contract with the id");

            var otpCode = Generator.GenerateVerificationCode();
            int seconds = 60;
            DateTime expireDate = DateTime.UtcNow.AddSeconds(seconds);
            rentalContract.OtpCode = otpCode;
            rentalContract.ContractStatus=ContractStatusEnum.Unsigned.ToString();
            rentalContract.ExpireAt = expireDate;
            _unitOfWork.GetRentalContractRepository().Update(rentalContract);
            await _unitOfWork.SaveChangesAsync();
            _ = Task.Run(() => _emailService.SendVerificationOtpAsync(renter.Email, otpCode, seconds));

            return ResultResponse<string>.SuccessResult($"Otp code sent, {seconds} to be expired", null);


        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure($"An error occurred while retrieving rental receipts: {ex.Message}");

        }
    }


    public async Task<ResultResponse<string>> ConfirmedRentalContract(Guid rentalContractId,string otpCode)
    {
        try
        {
            var rentalContract = await _unitOfWork.GetRentalContractRepository().GetRentalContractAsync(rentalContractId);

            var rentalReceipt = await _unitOfWork.GetRentalReceiptRepository().GetRentalReceiptWithReferences(rentalContract.Booking.RentalReceipt.Id);
            if (rentalReceipt == null&&rentalContract==null)
            {
                return ResultResponse<string>.Failure("Rental receipt and contract not found.");
            }
            if(rentalContract.OtpCode!=otpCode&& rentalContract.ExpireAt!=DateTime.Now)
            {
                return ResultResponse<string>.Failure("Otp code is expired or not correct.");
            }
            rentalContract.ExpireAt = null;
            rentalContract.OtpCode = string.Empty;
            rentalReceipt.RenterConfirmedAt = DateTime.UtcNow;
            rentalContract.ContractStatus = ContractStatusEnum.Signed.ToString();
            rentalContract.Booking.Vehicle.Status= VehicleStatusEnum.Rented.ToString();
            rentalContract.Booking.BookingStatus = BookingStatusEnum.Renting.ToString();
            _unitOfWork.GetVehicleRepository().Update(rentalContract.Booking.Vehicle);
            _unitOfWork.GetBookingRepository().Update(rentalContract.Booking);
            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);
            _unitOfWork.GetRentalContractRepository().Update(rentalContract);
            await _unitOfWork.SaveChangesAsync();
           
            return ResultResponse<string>.SuccessResult("Rental receipt confirmed successfully.", null);
        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure($"An error occurred while confirming the rental receipt: {ex.Message}");
        }
    }
    public async Task<ResultResponse<List<RentalReceiptResponse>>> GetAllRentalReceipt()
    {

        try
        {
            var medias = await _unitOfWork.GetMediaRepository().Query()
                .Where(a =>
                    a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListHandOver.ToString()
                    || a.EntityType == MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString()
                    || a.EntityType == MediaEntityTypeEnum.RentalReceiptReturnImage.ToString())
                .ToListAsync();

            var mediaDict = medias
                .GroupBy(a => a.DocNo)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rentalReceipts = await _unitOfWork.GetRentalReceiptRepository().GetAllAsync();

            var rentalReceiptResponse = rentalReceipts.Select(rentalReceipt =>
            {
                List<string> checkListFiles = new();
                List<string> handOverImages = new();
                List<string> returnImages = new();

                if (mediaDict.TryGetValue(rentalReceipt.Id, out var mediaFiles))
                {
                    foreach (var media in mediaFiles)
                    {
                        switch (media.EntityType)
                        {
                            case nameof(MediaEntityTypeEnum.RentalReceiptCheckListHandOver):
                                checkListFiles.Add(media.FileUrl);
                                break;

                            case nameof(MediaEntityTypeEnum.RentalReceiptHandoverImage):
                                handOverImages.Add(media.FileUrl);
                                break;

                            case nameof(MediaEntityTypeEnum.RentalReceiptReturnImage):
                                returnImages.Add(media.FileUrl);
                                break;
                        }
                    }
                }

                return new RentalReceiptResponse
                {
                    Id = rentalReceipt.Id,
                    StartBatteryPercentage = rentalReceipt.StartBatteryPercentage,
                    StartOdometerKm = rentalReceipt.StartOdometerKm,
                    EndOdometerKm = rentalReceipt.EndOdometerKm,
                    BookingId = rentalReceipt.BookingId,
                    Notes = rentalReceipt.Notes,
                    RenterConfirmedAt = rentalReceipt.RenterConfirmedAt,
                    StaffId = rentalReceipt.StaffId,
                    CheckListFile = checkListFiles,
                    HandOverVehicleImageFiles = handOverImages,
                    ReturnVehicleImageFiles = returnImages
                };
            }).ToList();

            return ResultResponse<List<RentalReceiptResponse>>.SuccessResult(
                "Rental receipts retrieved successfully", rentalReceiptResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<RentalReceiptResponse>>.Failure(
                $"An error occurred while retrieving rental receipts: {ex.Message}");
        }
    }
    public async Task<ResultResponse<string>> DeleteRentalReceiptAsync(Guid rentalReceiptId)
    {
        try
        {
            var rentalReceipt = await _unitOfWork.GetRentalReceiptRepository().FindByIdAsync(rentalReceiptId);
            if (rentalReceipt == null)
            {
                return ResultResponse<string>.Failure("Rental receipt not found.");
            }
            _unitOfWork.GetRentalReceiptRepository().Delete(rentalReceipt);
            await _unitOfWork.SaveChangesAsync();
            return ResultResponse<string>.SuccessResult("Rental receipt deleted successfully.", null);
        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure($"An error occurred while deleting the rental receipt: {ex.Message}");
        }
    }
    private bool IsBookingReadyForContract(Booking booking)
    {
        // booking null
        if (booking == null)
            return false;

        // chưa có hóa đơn thuê
        if (booking.RentalReceipt == null)
            return false;

        // chưa có chi nhánh giao xe
        if (booking.HandoverBranch == null)
            return false;

        // chưa có người thuê
        if (booking.Renter == null || booking.Renter.Account == null)
            return false;

        // chưa có xe
        if (booking.Vehicle == null || booking.Vehicle.VehicleModel == null)
            return false;

        // chưa có nhân viên bàn giao
        if (booking.RentalReceipt.Staff == null || booking.RentalReceipt.Staff.Account == null)
            return false;

        return true;
    } 
    public async Task<ResultResponse<RentalContractResponse>>GetContractAsync(Guid bookingId)
    {
        try
        {
            var rentalContract = await _unitOfWork.GetRentalContractRepository().GetRentalContractByBookingIdAsync(bookingId);
            var media = _unitOfWork.GetMediaRepository().GetAMediaWithCondAsync(rentalContract.Id, MediaEntityTypeEnum.RentalContract.ToString());
            
            var response = new RentalContractResponse
            {
                Id = rentalContract.Id,
                ContractStatus = rentalContract.ContractStatus,
                ExpireAt = DateTime.UtcNow,
                OtpCode = rentalContract.OtpCode,
                file = media.Result.FileUrl??string.Empty,

            };
            return ResultResponse<RentalContractResponse>.SuccessResult("RentalCotnract Founded", response);

        }
        catch (Exception ex)
        {
            return ResultResponse<RentalContractResponse>.Failure($"An error occurred while finding the rental receipt: {ex.Message}");

        }
    }
    public async Task<ResultResponse<List<RentalContractResponse>>> GetAllRentalContractsAsync()
    {
        try
        {
       
            var rentalContracts = await _unitOfWork
                .GetRentalContractRepository()
                .GetRentalContractsAsync();

            if (rentalContracts == null || !rentalContracts.Any())
                return ResultResponse<List<RentalContractResponse>>.Failure("No rental contracts found.");

            var contractIds = rentalContracts.Select(rc => rc.Id).ToList();

            var medias = await _unitOfWork.GetMediaRepository()
                .Query()
                .Where(m => m.EntityType == MediaEntityTypeEnum.RentalContract.ToString())
                .Where(m => contractIds.Contains(m.DocNo))
                .ToListAsync();

            var mediaDict = medias
                .GroupBy(m => m.DocNo)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            var responses = rentalContracts.Select(rc => new RentalContractResponse
            {
                Id = rc.Id,
                ContractStatus = rc.ContractStatus,
                ExpireAt = DateTime.UtcNow,
                OtpCode = rc.OtpCode,
                file = mediaDict.TryGetValue(rc.Id, out var media) ? media.FileUrl ?? string.Empty : string.Empty
            }).ToList();

            return ResultResponse<List<RentalContractResponse>>.SuccessResult("Rental contracts retrieved successfully.", responses);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<RentalContractResponse>>.Failure($"Error while retrieving rental contracts: {ex.Message}");
        }
    }


    public async Task<ResultResponse<string>>DeleteContractAsync (Guid contractId)
    {
        try
        {
            var rentalContract= await _unitOfWork.GetRentalContractRepository().FindByIdAsync(contractId);
             _unitOfWork.GetRentalContractRepository().Delete(rentalContract);
            await _unitOfWork.SaveChangesAsync();
            return ResultResponse<string>.SuccessResult("Rental Contract Deleted", null);

        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure($"An error occurred while deleting the rental contract: {ex.Message}");

        }
    }

    public async Task<ResultResponse<RentalContractFileResponse>> CreateRentalContractAsync(Guid BookingId )
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository().GetBookingByIdWithReferencesAsync(BookingId);

            string name = $"HopDongThueXe_GSM_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            if (booking.RentalContract!=null)
            {
                return ResultResponse<RentalContractFileResponse>.Failure(
                  "Booking already has contract"
              );
            }
            if (!IsBookingReadyForContract(booking))
            {
                return ResultResponse<RentalContractFileResponse>.Failure(
                    "Booking chưa đủ dữ liệu để tạo hợp đồng. Vui lòng kiểm tra thông tin RentalReceipt, Vehicle, Renter hoặc Branch."
                );
            }

            var rentalContract = new RentalContract
            {
                OtpCode=string.Empty,
                ContractStatus = ContractStatusEnum.Unsigned.ToString(),
                BookingId= BookingId,
            };
            var contractData = new ContractData
            {
                ContractDate =  DateTimeExtensions.ToVietnamTimeString(booking.RentalReceipt.RenterConfirmedAt),
                ContractLocation = booking.HandoverBranch.Address,
                DeliveryLocationName = booking.HandoverBranch.Address,
                LesseeDriverId = (booking.Renter.Documents
                .Where(d=>d.DocumentType==DocumentTypeEnum.Citizen.ToString()).FirstOrDefault())?.DocumentNumber
                ??"0000000000",
                LesseeDriverName=booking.Renter.Account.Fullname,
                LesseeDriverPhone=booking.Renter.phone,
                LessorDeliveryStaffName=booking.RentalReceipt.Staff.Account.Fullname,
                LessorDeliveryStaffPosition=booking.RentalReceipt.Staff.Account.Role,
             
                LicensePlate=booking.Vehicle.LicensePlate,
                RegistrationIssueDate=DateTimeExtensions.ToVietnamTimeString(booking.Vehicle.PurchaseDate),
                RentalDay=DateTimeExtensions.ToVietnamTimeString(DateTime.Now),
                RentalPrice=booking.DepositAmount,
                VehicleColor=booking.Vehicle.Color,
                VehicleModelName=booking.Vehicle.VehicleModel.ModelName,
                

            };

          
            var pdf =await _pdfGenerator.GeneratePdfAsync(contractData);
            if(pdf == null)
            {
                return ResultResponse<RentalContractFileResponse>.Failure("error generating contract.");

            }
            string fileUrl=await _cloudinaryService.UploadDocumentFileAsync(
                FileHelper.ConvertByteArrayToFormFile(pdf, name),
                 name,
                 "RentalContract"
                );
            Media media = new Media
            {
                FileUrl = fileUrl,
                DocNo = rentalContract.Id,
                EntityType = MediaEntityTypeEnum.RentalContract.ToString(),
                MediaType = MediaTypeEnum.Document.ToString(),
            };
            await _unitOfWork.GetMediaRepository().AddAsync(media);
            await _unitOfWork.GetRentalContractRepository().AddAsync(rentalContract);
            await _unitOfWork.SaveChangesAsync();

            RentalContractFileResponse response = new RentalContractFileResponse
            {
                FileData = pdf,
                Name = name
            };
            return ResultResponse<RentalContractFileResponse>.SuccessResult("Rental Contract Created", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<RentalContractFileResponse>.Failure($"An error occurred while deleting the rental receipt: {ex.Message}");

        }
    }
  
    public async Task<ResultResponse<RentalReceiptResponse>> CreateRentailReceiptAsync(RentalReceiptCreateRequest rentalReceiptCreateRequest)
    {
        try
        {
            var userId = Guid.Parse(_currentUserService.UserId);
           
            var rentalReceipt = new RentalReceipt
            {
                Id = Guid.NewGuid(),
                BookingId = rentalReceiptCreateRequest.BookingId,
                Notes = rentalReceiptCreateRequest.Notes,
                StaffId = userId,
                StartOdometerKm = rentalReceiptCreateRequest.StartOdometerKm,
                StartBatteryPercentage = rentalReceiptCreateRequest.StartBatteryPercentage,
                
            };
            
            var url = await _cloudinaryService.UploadImageFileAsync(
                rentalReceiptCreateRequest.CheckListFile,
                $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                "RentalReceipt"
                );
            var checklistmedia = new Media
            {
                FileUrl = url,
                DocNo = rentalReceipt.Id,
                EntityType = MediaEntityTypeEnum.RentalReceiptCheckListHandOver.ToString(),
                MediaType = MediaTypeEnum.Image.ToString(),
            };


            var uploadTasks = rentalReceiptCreateRequest.VehicleFiles.Select(async file =>
            {

                var url = await _cloudinaryService.UploadImageFileAsync(
                    file,
                    $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
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
            await _unitOfWork.SaveChangesAsync();
            var rentalReceiptResponse = new RentalReceiptResponse
            {
                Id = rentalReceipt.Id,
                StartOdometerKm = rentalReceipt.StartOdometerKm,
                StartBatteryPercentage = rentalReceipt.StartBatteryPercentage,
                BookingId = rentalReceipt.BookingId,
                Notes = rentalReceipt.Notes,
                RenterConfirmedAt = rentalReceipt.RenterConfirmedAt,
                StaffId = userId,
                HandOverVehicleImageFiles = uploadTasks.Select(file =>
                    file.Result.FileUrl).ToList(),
                CheckListFile = new List<string>(),
            };
            return ResultResponse<RentalReceiptResponse>.SuccessResult("Bookings retrieved successfully", rentalReceiptResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<RentalReceiptResponse>.Failure($"An error occurred while creating the rental receipt: {ex.Message}");
        }
    }
    public async Task<ResultResponse<RentalReceiptUpdateResponse>> UpdateRentalReceiptAsync(RentalReceiptUpdateRequest rentalReceiptUpdateRequest)
    {
        try
        {
            var foundedRentalReceipt = (_unitOfWork.GetRentalReceiptRepository().FindByIdAsync(rentalReceiptUpdateRequest.RentalReceiptId)).Result;
            if (foundedRentalReceipt == null)

                return ResultResponse<RentalReceiptUpdateResponse>.NotFound("can not find the receipt");
            string? checkListMediaUrl = await _cloudinaryService.UploadImageFileAsync(
                rentalReceiptUpdateRequest.ReturnCheckListFile,
                $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                "RentalReceipt"
                );
            var checkListMedia= new Media
            {
                EntityType = MediaEntityTypeEnum.RentalReceiptCheckListReturn.ToString(),
                FileUrl = checkListMediaUrl,
                DocNo = foundedRentalReceipt.Id,
                MediaType = MediaTypeEnum.Image.ToString(),
            };
            var returnVehicleImages = rentalReceiptUpdateRequest.ReturnVehicleImagesFiles.Select(async a =>
            {
                var url = await _cloudinaryService.UploadImageFileAsync(
                    a,
                     $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                     "RentalReceipt"
                    );
                return new Media
                {
                    EntityType = MediaEntityTypeEnum.RentalReceiptReturnImage.ToString(),
                    FileUrl = url,
                    DocNo = foundedRentalReceipt.Id,
                    MediaType = MediaTypeEnum.Image.ToString(),
                };
            }).ToList();
            List<Media> medias = (await Task.WhenAll(returnVehicleImages)).ToList();
            foundedRentalReceipt.EndBatteryPercentage = rentalReceiptUpdateRequest.EndBatteryPercentage;
            foundedRentalReceipt.EndOdometerKm = rentalReceiptUpdateRequest.EndOdometerKm;
            await _unitOfWork.GetMediaRepository().AddRangeAsync(medias);
            await _unitOfWork.GetMediaRepository().AddAsync(checkListMedia);
            _unitOfWork.GetRentalReceiptRepository().Update(foundedRentalReceipt);
            await _unitOfWork.SaveChangesAsync();
           
           
            var rentalReceiptResponse = new RentalReceiptUpdateResponse
            {
                Id = foundedRentalReceipt.Id,
            
                ReturnVehicleImageFiles= returnVehicleImages.Select(file =>
                    file.Result.FileUrl).ToList(),
                CheckListFile = checkListMediaUrl,
                EndOdometerKm= foundedRentalReceipt.EndOdometerKm,
                EndBatteryPercentage=foundedRentalReceipt.EndBatteryPercentage
            };
            return ResultResponse<RentalReceiptUpdateResponse>.SuccessResult("Bookings retrieved successfully", rentalReceiptResponse);

        }
        catch (Exception ex)
        {
            return ResultResponse<RentalReceiptUpdateResponse>.Failure($"An error occurred {ex.Message}");

        }
    }
}
