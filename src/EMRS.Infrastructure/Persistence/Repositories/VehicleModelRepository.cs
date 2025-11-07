using EMRS.Application.Common;
using EMRS.Application.DTOs.VehicleModelDTOs;
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

public class VehicleModelRepository:GenericRepository<VehicleModel>,IVehicleModelRepository
{
    private readonly EMRSDbContext _context;
    public VehicleModelRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<IEnumerable<VehicleModel>> GetVehicleModelsWithReferencesAsync()
    {
        return await Query()
            .AsNoTracking()
            .Include(v => v.RentalPricing)
                .Include(vm => vm.Vehicles)
           .ToListAsync();
    }
    public async Task<VehicleModel?> GetVehicleModelWithReferencesByIdAsync(Guid vehicleModelId)
    {
        return await Query()
            .AsNoTracking()
            .Include(v => v.RentalPricing)
          .FirstOrDefaultAsync(v => v.Id == vehicleModelId);
    }
   
    public PaginationResult<List<VehicleModel>> SearchAvailableModels(
      VehicleModelSearchRequest request, int PageSize, int PageNum)
    {

        if (PageNum <= 0) PageNum = 1;
        if (PageSize <= 0) PageSize = 1;


        var query = _context.VehicleModels.AsQueryable();

        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            var start = request.StartDate.Value;
            var end = request.EndDate.Value;

            query = query.Where(vm =>
                vm.Vehicles.Any(v =>
                    (request.BranchId == null || v.BranchId == request.BranchId) &&
                    v.Status == VehicleStatusEnum.Available.ToString() &&
                    !v.Bookings.Any(b =>
                        b.BookingStatus != BookingStatusEnum.Canceled.ToString() &&
                        b.BookingStatus != BookingStatusEnum.Completed.ToString() &&
                        b.StartDatetime < end &&
                        b.EndDatetime > start
                    )
                )
            );
        }
        else if (request.BranchId.HasValue)
        {
            query = query.Where(vm => vm.Vehicles
            .Any(v => v.BranchId == request.BranchId && v.Status == VehicleStatusEnum.Available.ToString()));
        }
        else
        {
            query = Query();
        }


            var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / PageSize);

        var items = new List<VehicleModel>();
        if (PageSize == 0 && PageNum == 0)
        {
            items = query
                .Include(vm=>vm.Vehicles)
            .Include(vm => vm.RentalPricing)
            .ToList();

        }
        else
        {
          
            items = query
                .Include(vm=>vm.Vehicles)
                .Include(vm => vm.RentalPricing)
                .Skip((PageNum - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
        

        return new PaginationResult<List<VehicleModel>>
        {
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = PageNum,
            PageSize = PageSize,
            Items = items
        };
    }




}
