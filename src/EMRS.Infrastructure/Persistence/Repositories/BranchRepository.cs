using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
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

public class BranchRepository: GenericRepository<Branch>, IBranchRepository
{
    private readonly EMRSDbContext _context;
    public BranchRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<List<Branch>> SearchBranchWithAvailableModelsAsync(
    BranchSearchRequest request)
    {
     

        var query = Query();

        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            var start = request.StartDate.Value;
            var end = request.EndDate.Value;

            query = query.Where(vm =>
                vm.Vehicles.Any(v =>
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
        else
        {
            query = query.Where(vm =>
                vm.Vehicles.Any(v =>
                    v.Status == VehicleStatusEnum.Available.ToString()));
        }

           var totalItems = await query.CountAsync();

        var items = await query.ToListAsync();

        return items?? new List<Branch>();
    }
}
