using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
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
    public async Task<ResultResponse<RentalReceiptResponse>> GetAllByBookingIdAsync(Guid bookingId)
    {
        try
        {
            var rentalReceipts = await _unitOfWork.GetRentalReceiptRepository().GetRentalReceiptByBookingId(bookingId);
            if (rentalReceipts==null)
            {
                return ResultResponse<RentalReceiptResponse>.NotFound("There are not rental receipt found");

            }
            var rentalReceiptResponse = _mapper.Map<List<RentalReceiptResponse>>(rentalReceipts);
            var medias= await _unitOfWork.GetMediaRepository().Query().Where(a=>a.DocNo==bookingId)
                .Where(a=>a.EntityType==MediaEntityTypeEnum.RentalReceiptCheckList.ToString()
                 || a.EntityType == MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString()).ToListAsync();
            var mediaDitct= medias
                .GroupBy(a=>a.DocNo)
                .ToDictionary(g=>g.Key,g=>g.ToList());
            var value=mediaDitct.TryGetValue(bookingId,out var vehicleFiles);
            var response = new RentalReceiptResponse
            {
                StaffId = rentalReceipts.StaffId,
                StartBatteryPercentage = rentalReceipts.StartBatteryPercentage,
                StartOdometerKm = rentalReceipts.StartOdometerKm,
                EndOdometerKm = rentalReceipts.EndOdometerKm,
                BookingId = rentalReceipts.BookingId,
                Notes = rentalReceipts.Notes,
                RenterConfirmedAt = rentalReceipts.RenterConfirmedAt,
                VehicleFiles = value ? vehicleFiles
                    .Where(a => a.EntityType == MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString())
                    .Select(m => m.FileUrl).ToList() : new List<string>(),
                CheckListFile = value?vehicleFiles
                    .Where(a => a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckList.ToString())
                    .Select(m => m.FileUrl).FirstOrDefault():null,
            };

            return ResultResponse<RentalReceiptResponse>.SuccessResult("There is a rental receipt found",response);

        }
        catch (Exception ex)
        {
            return ResultResponse<RentalReceiptResponse>.Failure($"An error occurred while retrieving rental receipts: {ex.Message}");
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
            rentalContract.ContractStatus=ContractStatusEnum.Signed.ToString();
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


    public async Task<ResultResponse<string>> ConfirmedRentalReceipt(Guid rentalReceiptId,string otpCode)
    {
        try
        {
            var rentalReceipt = await _unitOfWork.GetRentalReceiptRepository().GetRentalReceiptWithReferences(rentalReceiptId);
            var rentalContract =  rentalReceipt.Booking.RentalContract;
            if (rentalReceipt == null&&rentalContract==null)
            {
                return ResultResponse<string>.Failure("Rental receipt and contract not found.");
            }
            if(rentalContract.OtpCode!=otpCode&& rentalContract.ExpireAt!=DateTime.Now)
            {
                return ResultResponse<string>.Failure("Otp code is expired or not correct.");
            }
            rentalContract.ExpireAt = null;
            rentalContract.OtpCode = null;
            rentalReceipt.RenterConfirmedAt = DateTime.UtcNow;
            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);
            _unitOfWork.GetRentalContractRepository().Update(rentalContract);
            await _unitOfWork.SaveChangesAsync();
            var rentalReceiptResponse = _mapper.Map<RentalReceiptResponse>(rentalReceipt);
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

            var medias = await _unitOfWork.GetMediaRepository().Query().Where(a =>
                 a.EntityType == MediaEntityTypeEnum.RentalReceiptCheckList.ToString()
                 || a.EntityType == MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString()
               || a.EntityType == MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()).ToListAsync();
            var mediaDict = medias
             .GroupBy(a => a.DocNo)
             .ToDictionary(g => g.Key, g => g.ToList());
            var rentalReceipts = await _unitOfWork.GetRentalReceiptRepository().GetAllAsync();
            var rentalReceiptResponse= rentalReceipts.Select( rentalReceipt => new RentalReceiptResponse
            {
                Id= rentalReceipt.Id,
                StartBatteryPercentage = rentalReceipt.StartBatteryPercentage,
                StartOdometerKm = rentalReceipt.StartOdometerKm,
                EndOdometerKm = rentalReceipt.EndOdometerKm,
                BookingId = rentalReceipt.BookingId,
                Notes = rentalReceipt.Notes,
                RenterConfirmedAt = rentalReceipt.RenterConfirmedAt,
                StaffId = rentalReceipt.StaffId,
                CheckListFile=mediaDict.TryGetValue(rentalReceipt.Id,out var Checlistfile)? 
                    Checlistfile.Where(a=>a.EntityType==MediaEntityTypeEnum.RentalReceiptCheckList.ToString()).Select(m=>m.FileUrl).FirstOrDefault() : null,
                VehicleFiles = mediaDict.TryGetValue(rentalReceipt.Id, out var vehicleFiles) ?
                    vehicleFiles.Where(a => a.EntityType != MediaEntityTypeEnum.RentalReceiptCheckList.ToString()).Select(m => m.FileUrl).ToList() : new List<string>(),
                


            }).ToList() ?? new List<RentalReceiptResponse>();
            return ResultResponse<List<RentalReceiptResponse>>.SuccessResult("Rental receipts retrieved successfully", rentalReceiptResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<RentalReceiptResponse>>.Failure($"An error occurred while retrieving rental receipts: {ex.Message}");
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
    public async Task<ResultResponse<RentalContractResponse>>GetContractAsync(Guid contractId)
    {
        try
        {
            var rentalContract = await _unitOfWork.GetRentalContractRepository().GetRentalContractAsync(contractId);
            var media = _unitOfWork.GetMediaRepository().GetAMediaWithCondAsync(contractId, MediaEntityTypeEnum.RentalContract.ToString());
            var response = new RentalContractResponse
            {
                Id = contractId,
                ContractStatus = rentalContract.ContractStatus,
                ExpireAt = DateTime.UtcNow,
                OtpCode = rentalContract.OtpCode,
                file = media.Result.FileUrl,

            };
            return ResultResponse<RentalContractResponse>.SuccessResult("RentalCotnract Founded", response);

        }
        catch (Exception ex)
        {
            return ResultResponse<RentalContractResponse>.Failure($"An error occurred while deleting the rental receipt: {ex.Message}");

        }
    }
   
    public async Task<ResultResponse<string>>DeleteContractAsync (Guid contractId)
    {
        try
        {
            var rentalContract= await _unitOfWork.GetRentalContractRepository().FindByIdAsync(contractId);
             _unitOfWork.GetRentalContractRepository().Delete(rentalContract);
            return ResultResponse<string>.SuccessResult("Rental Contract Deleted", null);

        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure($"An error occurred while deleting the rental receipt: {ex.Message}");

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
            
            string fileUrl=await _cloudinaryService.UploadDocumentFileAsync(
                FileHelper.ConvertByteArrayToFormFile(pdf, name),
                 name,
                 MediaTypeEnum.Document.ToString()
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
