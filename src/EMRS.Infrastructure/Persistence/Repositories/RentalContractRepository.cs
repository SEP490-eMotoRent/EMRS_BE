using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class RentalContractRepository:GenericRepository<RentalContract>, IRentalContractRepository
{
    private readonly EMRSDbContext _dbContext;
    public RentalContractRepository (EMRSDbContext eMRSDbContext): base(eMRSDbContext)
    {
        _dbContext = eMRSDbContext;
    }

    public async Task<RentalContract?> GetRentalContractAsync(Guid rentalContractId)
    {
        return await Query().Include(v=>v.Booking).SingleOrDefaultAsync(v=>v.Id==rentalContractId);
    }
    public async Task<IEnumerable<RentalContract>> GetRentalContractsAsync()
    {
        return await Query().Include(v => v.Booking).ToListAsync();
    }
    public async Task<RentalContract?> GetRentalContractByBookingIdAsync(Guid bookingId)
    {
        return await Query().Include(v => v.Booking).SingleOrDefaultAsync(v => v.BookingId == bookingId);
    }
}
