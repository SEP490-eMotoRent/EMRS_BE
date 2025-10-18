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
   
    public async Task<Account?> GetByEmaiAsync(string email)
    {
        var accounts = Query();
        var account= await  accounts.AsNoTracking().FirstOrDefaultAsync(
            a => a.Renter != null && 
            a.Renter.Email == email);
        return account;
    }
    
}
