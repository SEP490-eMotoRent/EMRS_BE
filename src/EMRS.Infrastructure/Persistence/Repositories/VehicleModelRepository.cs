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


}
