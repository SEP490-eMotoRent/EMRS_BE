using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class StaffRepository:GenericRepository<Staff>, IStaffRepository
{
    private readonly EMRSDbContext _context;
    public StaffRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Staff?> GetStaffByAccountIdAsync(Guid accountId)
    {
        return await Query()
            .Include(s => s.Account)
            .Include(s => s.Branch)
            .Where(s => s.AccountId == accountId)
            .FirstOrDefaultAsync();
    }
}
