using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public  class AccountRepository: GenericRepository<Account>, IAccountRepository
{
    private readonly EMRSDbContext _context;
    public AccountRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }
   
    public async Task<bool> GetByEmaiAndUsernameAsync(string email,string username)
    {
        var check = await Query().AnyAsync(
            a => a.Renter != null &&
            a.Renter.Email == email &&
            a.Username == username);
        return check;
    }


    public async Task<Account?> LoginAsync(string username)
    {
        var account = await Query()
     .Include(a => a.Renter)
     .Include(a => a.Staff)
     .ThenInclude(s => s.Branch)
     .AsNoTracking() 
     .SingleOrDefaultAsync(a => a.Username == username);


        return account;
    }
    
}
