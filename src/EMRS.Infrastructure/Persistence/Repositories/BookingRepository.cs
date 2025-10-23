using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class BookingRepository:GenericRepository<Booking>, IBookingRepository
{
    private readonly EMRSDbContext _dbContext;
    public BookingRepository(EMRSDbContext dbContext):base(dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<Booking>> GetBookingsByRenterIdAsync(Guid renterId)
    {
        return await Query().Where(Query => Query.Id == renterId).ToListAsync();
    }
}
