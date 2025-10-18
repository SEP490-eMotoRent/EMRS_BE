using AutoMapper;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
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

        //RentalPricing
        CreateMap<RentalPricing, CreateRentalPricingRequest>();
        CreateMap<RentalPricing, RentalPricingResponse>();
        //Vehicle
        CreateMap<Vehicle, VehicleResponse>()
  .ForMember(dest => dest.rentalPricing,
      opt => opt.MapFrom(src => src.VehicleModel.RentalPricing));
        /*.ForMember()*/


    }
}
