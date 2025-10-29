using EMRS.Application.Common;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class VehicleRepository:GenericRepository<Vehicle>, IVehicleRepository
{
    private readonly EMRSDbContext _context;
    public VehicleRepository(EMRSDbContext context):base(context)
    {
        _context = context;
    }
    public async Task<Vehicle?> GetVehicleWithReferencesAsync(Guid vehicleId, Guid vehicleModelId)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Include(v => v.VehicleModel)
                .ThenInclude(vm => vm.RentalPricing)
            .SingleOrDefaultAsync(v => v.Id == vehicleId && v.VehicleModel.Id == vehicleModelId);
    }
    public async Task<Vehicle?> GetVehicleWithReferences2Async(Guid vehicleId)
    {
        return await Query()
            .Include(v=>v.Branch)
            .Include(v => v.VehicleModel)
                .ThenInclude(vm => vm.RentalPricing)
            .SingleOrDefaultAsync(v => v.Id == vehicleId);
    }
    public async Task<Vehicle?> GetOneRandomVehicleAsync(Guid VehicleModelId)
    {
        var result =  await Query()
            .Where(v => v.VehicleModelId == VehicleModelId
                        && v.Status == VehicleStatusEnum.Available.ToString())
            .OrderBy(v => Guid.NewGuid()).FirstOrDefaultAsync();



        return result;
    }


    public async Task<PaginationResult<List<Vehicle>>> GetVehicleListWithReferencesAsync(
        VehicleSearchRequest vehicleSearchRequest, int PageSize, int PageNum)

    {
        if (PageNum <= 0) PageNum = 1;
        if (PageSize <= 0) PageSize = 1;
        var searchResult = Query().Include(v => v.VehicleModel).ThenInclude(vm => vm.RentalPricing).


            Where(v =>
         (string.IsNullOrEmpty(vehicleSearchRequest.LicensePlate) || v.LicensePlate.Contains(vehicleSearchRequest.LicensePlate)) &&
         (string.IsNullOrEmpty(vehicleSearchRequest.Color) || v.Color.Contains(vehicleSearchRequest.Color)) &&
         (string.IsNullOrEmpty(vehicleSearchRequest.Status) || v.Status == vehicleSearchRequest.Status) &&
         (!vehicleSearchRequest.CurrentOdometerKm.HasValue || v.CurrentOdometerKm == vehicleSearchRequest.CurrentOdometerKm) &&
         (!vehicleSearchRequest.BatteryHealthPercentage.HasValue || v.BatteryHealthPercentage == vehicleSearchRequest.BatteryHealthPercentage) &&
         (string.IsNullOrEmpty(vehicleSearchRequest.BranchId.ToString()) || v.BranchId == vehicleSearchRequest.BranchId) &&
         (string.IsNullOrEmpty(vehicleSearchRequest.VehicleModelId.ToString()) || v.VehicleModelId == vehicleSearchRequest.VehicleModelId)
        );
        var totalCount = await searchResult.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);
        searchResult = searchResult.Skip((PageNum - 1) * PageSize).Take(PageSize);
        var PaginationResult = new PaginationResult<List<Vehicle>>
        {
            CurrentPage = PageNum,
            PageSize = PageSize,
            TotalPages = totalPages,
            Items = await searchResult.ToListAsync(),
            TotalItems = totalCount
        };
        return PaginationResult ?? new PaginationResult<List<Vehicle>>();
    }
  


}
