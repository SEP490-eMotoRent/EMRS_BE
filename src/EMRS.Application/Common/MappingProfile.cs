using AutoMapper;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.InsuranceClaimDTOs;
using EMRS.Application.DTOs.InsurancePackageDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
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

    }
}
