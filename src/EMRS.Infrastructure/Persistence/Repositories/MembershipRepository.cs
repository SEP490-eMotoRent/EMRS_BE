using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class MembershipRepository:GenericRepository<Membership>, IMembershipRepository

{
    private readonly EMRSDbContext _context;
    public MembershipRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<Membership?> FindLowestMinBookingMembershipAsync()
    {
        return await Query()                         
            .OfType<Membership>()                   
            .Where(m => m.MinBookings == 0)         
            .OrderBy(m => m.MinBookings)            
            .FirstOrDefaultAsync();                 
    }

}
