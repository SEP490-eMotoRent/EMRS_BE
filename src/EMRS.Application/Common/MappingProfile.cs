using AutoMapper;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.ChargingRecordDTOs;
using EMRS.Application.DTOs.InsuranceClaimDTOs;
using EMRS.Application.DTOs.InsurancePackageDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.DTOs.StaffDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Application.DTOs.VehicleTransferDTOs;
using EMRS.Application.DTOs.WalletDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Common;

public class MappingProfile:Profile
{
    public MappingProfile()
    {
        //renter
        CreateMap<Renter, RegisterRenterResponse>();
        //Branch
        CreateMap<Branch, BranchResponse>();
        //VehicleModel

        CreateMap<VehicleModel, VehicleModelResponse>();

        //Booking
        CreateMap<Booking, BookingResponse>();
        //RentalPricing
        CreateMap<RentalPricing, CreateRentalPricingRequest>();
        CreateMap<RentalPricing, RentalPricingResponse>();
        //Vehicle
        CreateMap<Vehicle, VehicleResponse>()
  .ForMember(dest => dest.rentalPricing,
      opt => opt.MapFrom(src => src.VehicleModel.RentalPricing));
        //Wallet
        CreateMap<Wallet, WalletResponse>();
        //Insurance Package
        CreateMap<InsurancePackage, InsurancePackageResponse>();
        //Insurance Claim
        CreateMap<InsuranceClaim, InsuranceClaimResponse>();
        CreateMap<InsuranceClaim, InsuranceClaimDetailResponse>();


        // RentalReceipt mappings
        CreateMap<RentalReceipt, ReturnInitResponse>()
            .ForMember(dest => dest.RentalReceiptId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.HandoverTime, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<Booking, ReturnInitResponse>()
            .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RenterName, opt => opt.MapFrom(src => src.Renter.Account.Fullname))
            .ForMember(dest => dest.RenterPhone, opt => opt.MapFrom(src => src.Renter.phone))
            .ForMember(dest => dest.RenterEmail, opt => opt.MapFrom(src => src.Renter.Email))
            .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.Vehicle.LicensePlate))
            .ForMember(dest => dest.VehicleModelName, opt => opt.MapFrom(src => src.Vehicle.VehicleModel.ModelName))
            .ForMember(dest => dest.VehicleColor, opt => opt.MapFrom(src => src.Vehicle.Color));

        // Charging Record
        CreateMap<Account, CreateAccountResponse>()
    .ForMember(dest => dest.StaffId,
        opt => opt.MapFrom(src => src.Staff != null ? src.Staff.Id : (Guid?)null));

        CreateMap<ChargingRecord, ChargingRecordResponse>()
       .ForMember(dest => dest.BatteryPercentageCharged,
           opt => opt.MapFrom(src => src.EndBatteryPercentage - src.StartBatteryPercentage))
       .ForMember(dest => dest.TimeSlot,
           opt => opt.Ignore()); // TimeSlot được tính trong service

        // VehicleTransferRequest mappings
        CreateMap<VehicleTransferRequest, VehicleTransferRequestResponse>()
            .ForMember(dest => dest.VehicleModelName,
                opt => opt.MapFrom(src => src.VehicleModel.ModelName))
            .ForMember(dest => dest.StaffName,
                opt => opt.MapFrom(src => src.Staff.Account.Fullname))
            .ForMember(dest => dest.BranchName,
                opt => opt.MapFrom(src => src.Staff.Branch != null ? src.Staff.Branch.BranchName : "N/A"));

        // Add detailed mapping with nested objects
        CreateMap<VehicleTransferRequest, VehicleTransferRequestDetailResponse>()
            .ForMember(dest => dest.VehicleModel,
                opt => opt.MapFrom(src => src.VehicleModel))
            .ForMember(dest => dest.Staff,
                opt => opt.MapFrom(src => src.Staff))
            .ForMember(dest => dest.VehicleTransferOrder,
                opt => opt.MapFrom(src => src.VehicleTransferOrder));

        // VehicleTransferOrder mappings
        CreateMap<VehicleTransferOrder, VehicleTransferOrderResponse>()
            .ForMember(dest => dest.VehicleLicensePlate,
                opt => opt.MapFrom(src => src.Vehicle.LicensePlate))
            .ForMember(dest => dest.FromBranchName,
                opt => opt.MapFrom(src => src.FromBranch.BranchName))
            .ForMember(dest => dest.ToBranchName,
                opt => opt.MapFrom(src => src.ToBranch.BranchName));

        //  Add detailed mapping with nested objects
        CreateMap<VehicleTransferOrder, VehicleTransferOrderDetailResponse>()
            .ForMember(dest => dest.Vehicle,
                opt => opt.MapFrom(src => src.Vehicle))
            .ForMember(dest => dest.FromBranch,
                opt => opt.MapFrom(src => src.FromBranch))
            .ForMember(dest => dest.ToBranch,
                opt => opt.MapFrom(src => src.ToBranch))
            .ForMember(dest => dest.VehicleTransferRequests,
                opt => opt.MapFrom(src => src.VehicleTransferRequests));

    }
}
