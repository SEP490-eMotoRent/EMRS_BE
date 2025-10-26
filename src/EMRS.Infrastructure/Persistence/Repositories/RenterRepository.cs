using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class RenterRepository:GenericRepository<Renter>,IRenterRepository
{
    private readonly EMRSDbContext _context;
    public RenterRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Renter> GetRenterByAccountIdAsync(Guid renterId)
    {
        return await _context.Renters.Include(n=>n.Account).SingleOrDefaultAsync(r => r.Id == renterId);
    }
}
