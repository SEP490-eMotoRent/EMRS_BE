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
    public async Task<ResultResponse<List<RentalReceiptResponse>>> GetRentalReceiptDetailByBookingIdAsync(Guid bookingId)
    {
        try
        {
            var receipts = await _unitOfWork
           .GetRentalReceiptRepository()
           .GetRentalReceiptByBookingId(bookingId);

            if (receipts == null || !receipts.Any())
            {
                return ResultResponse<List<RentalReceiptResponse>>
                    .NotFound("There are no rental receipts found");
            }

            var receiptIds = receipts.Select(x => x.Id).ToList();

            var medias = await _unitOfWork.GetMediaRepository().Query()
                .Where(a =>
                    receiptIds.Contains(a.DocNo) &&
                    (a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListHandOver.ToString() ||
                     a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListReturn.ToString() ||
                     a.EntityType == MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString() ||
                     a.EntityType == MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()))
                .ToListAsync();

            // Nếu medias null hoặc empty => tạo dictionary rỗng
            var mediaLookup = (medias ?? new List<Media>())
                .GroupBy(m => m.DocNo)
                .ToDictionary(g => g.Key, g => g.ToList());

            var responseList = receipts.Select(receipt =>
            {
                var response = new RentalReceiptResponse
                {
                    Id = receipt.Id,
                    StaffId = receipt.StaffId,
                    StartBatteryPercentage = receipt.StartBatteryPercentage,
                    StartOdometerKm = receipt.StartOdometerKm,
                    EndOdometerKm = receipt.EndOdometerKm,
                    BookingId = receipt.BookingId,
                    Notes = receipt.Notes,
                    RenterConfirmedAt = DateTimeHelper.ToVietnamTime(receipt.RenterConfirmedAt),
                    EndBatteryPercentage = receipt.EndBatteryPercentage,
                    VehicleId = receipt.VehicleId,
                    HandOverVehicleImageFiles = new List<string>(),
                    ReturnVehicleImageFiles = new List<string>(),
                    CheckListHandoverFile = new List<string>(),
                    CheckListReturnFile = new List<string>()
                };

                if (mediaLookup.TryGetValue(receipt.Id, out var mList) && mList != null)
                {
                    foreach (var m in mList)
                    {
                        if (m == null || m.EntityType == null) continue;

                        switch (m.EntityType)
                        {
                            case nameof(MediaEntityTypeEnum.RentalReceiptHandoverImage):
                                response.HandOverVehicleImageFiles.Add(m.FileUrl ?? string.Empty);
                                break;

                            case nameof(MediaEntityTypeEnum.RentalReceiptReturnImage):
                                response.ReturnVehicleImageFiles.Add(m.FileUrl ?? string.Empty);
                                break;

                            case nameof(MediaEntityTypeEnum.RentalReceiptCheckListHandOver):
                                response.CheckListHandoverFile.Add(m.FileUrl ?? string.Empty);
                                break;

                            case nameof(MediaEntityTypeEnum.RentalReceiptCheckListReturn):
                                response.CheckListReturnFile.Add(m.FileUrl ?? string.Empty);
                                break;
                        }
                    }
                }

                return response;
            }).ToList();

            return ResultResponse<List<RentalReceiptResponse>>.SuccessResult("There is a rental receipt found", responseList);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<RentalReceiptResponse>>.Failure(
                $"An error occurred while retrieving rental receipts: {ex.Message}"
            );
        }
    }
    public async Task<ResultResponse<RentalReceiptResponse>> GetRentalReceiptDetailByIdAsync(Guid rentalReceiptId)
    {
        try
        {
            var receipt = await _unitOfWork
                .GetRentalReceiptRepository()
                .FindByIdAsync(rentalReceiptId);

            if (receipt == null)
            {
                return ResultResponse<RentalReceiptResponse>
                    .NotFound("Rental receipt not found.");
            }

            var medias = await _unitOfWork.GetMediaRepository().Query()
                .Where(a =>
                    a.DocNo == receipt.Id &&
                    (a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListHandOver.ToString() ||
                     a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListReturn.ToString() ||
                     a.EntityType == MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString() ||
                     a.EntityType == MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()))
                .ToListAsync();

            var response = new RentalReceiptResponse
            {
                Id = receipt.Id,
                StaffId = receipt.StaffId,
                StartBatteryPercentage = receipt.StartBatteryPercentage,
                StartOdometerKm = receipt.StartOdometerKm,
                EndOdometerKm = receipt.EndOdometerKm,
                BookingId = receipt.BookingId,
                Notes = receipt.Notes,
                RenterConfirmedAt = DateTimeHelper.ToVietnamTime(receipt.RenterConfirmedAt),
                EndBatteryPercentage = receipt.EndBatteryPercentage,
                VehicleId = receipt.VehicleId,
                HandOverVehicleImageFiles = new List<string>(),
                ReturnVehicleImageFiles = new List<string>(),
                CheckListHandoverFile = new List<string>(),
                CheckListReturnFile = new List<string>()
            };

            if (medias != null && medias.Any())
            {
                foreach (var m in medias)
                {
                    if (m == null || m.EntityType == null) continue;

                    switch (m.EntityType)
                    {
                        case nameof(MediaEntityTypeEnum.RentalReceiptHandoverImage):
                            response.HandOverVehicleImageFiles.Add(m.FileUrl ?? string.Empty);
                            break;

                        case nameof(MediaEntityTypeEnum.RentalReceiptReturnImage):
                            response.ReturnVehicleImageFiles.Add(m.FileUrl ?? string.Empty);
                            break;

                        case nameof(MediaEntityTypeEnum.RentalReceiptCheckListHandOver):
                            response.CheckListHandoverFile.Add(m.FileUrl ?? string.Empty);
                            break;

                        case nameof(MediaEntityTypeEnum.RentalReceiptCheckListReturn):
                            response.CheckListReturnFile.Add(m.FileUrl ?? string.Empty);
                            break;
                    }
                }
            }

            return ResultResponse<RentalReceiptResponse>
                .SuccessResult("Rental receipt retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<RentalReceiptResponse>
                .Failure($"An error occurred while retrieving rental receipt: {ex.Message}");
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


    public async Task<ResultResponse<string>> ConfirmedRentalContract(Guid rentalContractId,Guid rentalReceiptId,string otpCode)
    {
        try
        {
            var rentalContract = await _unitOfWork.GetRentalContractRepository().GetRentalContractAsync(rentalContractId);

            var rentalReceipt = await _unitOfWork.GetRentalReceiptRepository().GetRentalReceiptWithReferences(rentalReceiptId);
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
                    || a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListReturn.ToString()
                    || a.EntityType == MediaEntityTypeEnum.RentalReceiptReturnImage.ToString())
                .ToListAsync();

            var mediaDict = medias
                .GroupBy(a => a.DocNo)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rentalReceipts = await _unitOfWork.GetRentalReceiptRepository().GetAllAsync();

            var rentalReceiptResponse = rentalReceipts.Select(rentalReceipt =>
            {
                List<string> checkListHandoverFiles = new();
                List<string> checkListReturnFiles = new();
                List<string> handOverImages = new();
                List<string> returnImages = new();

                if (mediaDict.TryGetValue(rentalReceipt.Id, out var mediaFiles))
                {
                    foreach (var media in mediaFiles)
                    {
                        switch (media.EntityType)
                        {
                            case nameof(MediaEntityTypeEnum.RentalReceiptCheckListHandOver):
                                checkListHandoverFiles.Add(media.FileUrl);
                                break;
                            case nameof(MediaEntityTypeEnum.RentalReceiptCheckListReturn):
                                checkListReturnFiles.Add(media.FileUrl);
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
                    RenterConfirmedAt = DateTimeHelper.ToVietnamTime( rentalReceipt.RenterConfirmedAt),
                    StaffId = rentalReceipt.StaffId,
                    EndBatteryPercentage=rentalReceipt.EndBatteryPercentage,
                    VehicleId=rentalReceipt.VehicleId,
                    CheckListReturnFile=checkListReturnFiles,
                    CheckListHandoverFile = checkListHandoverFiles,
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
    private bool IsBookingReadyForContract(Booking booking)
    {
        // booking null
        if (booking == null)
            return false;

        // chưa có hóa đơn thuê
        if (booking.RentalReceipts == null)
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



        return true;
    }
    public async Task<ResultResponse<RentalContractFileResponse>> CreateRentalContractAsync(Guid BookingId, Guid RentalReceiptId )
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository().GetBookingByIdWithReferencesAsync(BookingId);
            var rentalReceipt = await _unitOfWork.GetRentalReceiptRepository().GetRentalReceiptWithReferencesByIdAsync(RentalReceiptId);
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
                ContractDate = DateTimeExtensions.ToVietnamTimeString(DateTime.UtcNow),
                ContractLocation = booking.HandoverBranch.Address,
                DeliveryLocationName = booking.HandoverBranch.Address,
                LesseeDriverId = (booking.Renter.Documents
                .Where(d=>d.DocumentType==DocumentTypeEnum.Citizen.ToString()).FirstOrDefault())?.DocumentNumber
                ??"0000000000",
                LesseeDriverName=booking.Renter.Account.Fullname,
                LesseeDriverPhone=booking.Renter.phone,
                LessorDeliveryStaffName= rentalReceipt.Staff.Account.Fullname,
                LessorDeliveryStaffPosition= rentalReceipt.Staff.Account.Role,
             
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
  
    public async Task<ResultResponse<RentalReceiptCreateResponse>> CreateRentailReceiptAsync(RentalReceiptCreateRequest rentalReceiptCreateRequest)
    {
        try
        {
            var userId = Guid.Parse(_currentUserService.UserId);
            var booking = await _unitOfWork.GetBookingRepository().GetBookingByIdWithLessReferencesAsync(rentalReceiptCreateRequest.BookingId);
            if(booking.VehicleId==null|| booking.VehicleModelId==null)
            {
                return ResultResponse<RentalReceiptCreateResponse>.Failure("Booking chưa có xe được chỉ định.");
            }
            var rentalReceipt = new RentalReceipt
            {
                Id = Guid.NewGuid(),
                BookingId = rentalReceiptCreateRequest.BookingId,
                Notes = rentalReceiptCreateRequest.Notes,
                StaffId = userId,
                StartOdometerKm = rentalReceiptCreateRequest.StartOdometerKm,
                StartBatteryPercentage = rentalReceiptCreateRequest.StartBatteryPercentage,
                VehicleModelId = booking.VehicleModelId,
                VehicleId = booking.VehicleId.Value,
            };
            
            var url = await _cloudinaryService.UploadImageFileAsync(
                rentalReceiptCreateRequest.CheckListFile,
                $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                "RentalReceipt/handover"
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
                    "RentalReceipt/handover"
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
            var rentalReceiptResponse = new RentalReceiptCreateResponse
            {
                Id = rentalReceipt.Id,
                StartOdometerKm = rentalReceipt.StartOdometerKm,
                StartBatteryPercentage = rentalReceipt.StartBatteryPercentage,
                BookingId = rentalReceipt.BookingId,
                Notes = rentalReceipt.Notes,
                RenterConfirmedAt = DateTimeHelper.ToVietnamTime( rentalReceipt.RenterConfirmedAt),
                StaffId = userId,
                HandOverVehicleImageFiles = uploadTasks.Select(file =>
                    file.Result.FileUrl).ToList(),
                CheckListFile = new List<string>(),
                VehicleId = rentalReceipt.VehicleId,
                VehicleModelId = rentalReceipt.VehicleModelId,
            };
            return ResultResponse<RentalReceiptCreateResponse>.SuccessResult("Renter Created  successfully", rentalReceiptResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<RentalReceiptCreateResponse>.Failure($"An error occurred while creating the rental receipt: {ex.Message}");
        }
    }
    public async Task<ResultResponse<RentalReceiptCreateResponse>> CreateRentailReceiptForChangingAsync(RentalReceiptCreateVehicleChangingRequest rentalReceiptCreateRequest)
    {
        try
        {
            var userId = Guid.Parse(_currentUserService.UserId);
            var booking = await _unitOfWork.GetBookingRepository().GetBookingByIdWithLessReferencesAsync(rentalReceiptCreateRequest.BookingId);
            if (booking.VehicleId == null || booking.VehicleModelId == null)
            {
                return ResultResponse<RentalReceiptCreateResponse>.Failure("Booking chưa có xe được chỉ định.");
            }
            if (booking.BookingStatus!=BookingStatusEnum.Renting.ToString())
            {
                return ResultResponse<RentalReceiptCreateResponse>.Failure("Không được phép đổi xe nếu khách chưa thuê xe.");
            }
            var rentalReceipt = new RentalReceipt
            {
                Id = Guid.NewGuid(),
                BookingId = rentalReceiptCreateRequest.BookingId,
                Notes = rentalReceiptCreateRequest.Notes,
                StaffId = userId,
                StartOdometerKm = rentalReceiptCreateRequest.StartOdometerKm,
                StartBatteryPercentage = rentalReceiptCreateRequest.StartBatteryPercentage,
                VehicleModelId = booking.VehicleModelId,
                VehicleId = rentalReceiptCreateRequest.VehicleId,
            };

            var url = await _cloudinaryService.UploadImageFileAsync(
                rentalReceiptCreateRequest.CheckListFile,
                $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                "RentalReceipt/handover"
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
                    "RentalReceipt/handover"
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
            booking.VehicleId = rentalReceiptCreateRequest.VehicleId;
            _unitOfWork.GetBookingRepository().Update(booking);
            await _unitOfWork.GetRentalReceiptRepository().AddAsync(rentalReceipt);
            await _unitOfWork.GetMediaRepository().AddRangeAsync(medias);
            await _unitOfWork.GetMediaRepository().AddAsync(checklistmedia);
            await _unitOfWork.SaveChangesAsync();
            var rentalReceiptResponse = new RentalReceiptCreateResponse
            {
                Id = rentalReceipt.Id,
                StartOdometerKm = rentalReceipt.StartOdometerKm,
                StartBatteryPercentage = rentalReceipt.StartBatteryPercentage,
                BookingId = rentalReceipt.BookingId,
                Notes = rentalReceipt.Notes,
                RenterConfirmedAt = DateTimeHelper.ToVietnamTime(rentalReceipt.RenterConfirmedAt),
                StaffId = userId,
                HandOverVehicleImageFiles = uploadTasks.Select(file =>
                    file.Result.FileUrl).ToList(),
                CheckListFile = new List<string>(),
                VehicleId = rentalReceipt.VehicleId,
                VehicleModelId = rentalReceipt.VehicleModelId,
            };
            return ResultResponse<RentalReceiptCreateResponse>.SuccessResult("Renter created  successfully", rentalReceiptResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<RentalReceiptCreateResponse>.Failure($"An error occurred while creating the rental receipt: {ex.Message}");
        }
    }

}
