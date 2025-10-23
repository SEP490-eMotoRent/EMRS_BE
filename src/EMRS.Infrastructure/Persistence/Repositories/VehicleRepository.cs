using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
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
    public async Task<IEnumerable<Vehicle?>> GetVehicleListWithReferencesAsync()

    {
        return await _context.Vehicles
           .AsNoTracking()
           .Include(v => v.VehicleModel)
               .ThenInclude(vm => vm.RentalPricing)
               .ToListAsync();
    }



}
