using AutoMapper;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.InsurancePackageDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
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



    }
}
