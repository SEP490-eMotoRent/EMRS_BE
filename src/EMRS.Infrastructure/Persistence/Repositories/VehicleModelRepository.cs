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

    public IQueryable<VehicleModel> SearchAvailableModelsQuery(VehicleModelSearchRequest request)
    {
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
                        b.BookingStatus != BookingStatusEnum.Cancelled.ToString() &&
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

        return query
            .Where(vm => vm.Vehicles
                .Any(v =>  v.Status == VehicleStatusEnum.Available.ToString()))
            .Include(vm => vm.Vehicles)
            .Include(vm => vm.RentalPricing)
            .AsSplitQuery(); 
    }

    public async Task<PaginationResult<List<VehicleModel>>> SearchAvailableModelsPaginationAsync(
        VehicleModelSearchRequest request, int pageSize, int pageNum)
    {
        var query = SearchAvailableModelsQuery(request);

        var totalItems = await query.CountAsync(); 

        if (pageSize <= 0) pageSize = totalItems; 
        if (pageNum <= 0) pageNum = 1;

        var items = await query
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;

        return new PaginationResult<List<VehicleModel>>
        {
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = pageNum,
            PageSize = pageSize,
            Items = items
        };
    }





}
