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

public class WalletRepository:GenericRepository<Wallet>, IWalletRepository
{
    private readonly EMRSDbContext _dbContext;
    public WalletRepository(EMRSDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<bool> UpdateBalanceAsync(Guid accountId, decimal amount)
    {
        var updated = await _dbContext.Wallets
            .Where(w => w.Id == accountId)
            .ExecuteUpdateAsync(s => s.SetProperty(w => w.Balance, w => w.Balance + amount));

        return updated > 0;
    }
  
    public async Task<Wallet?> GetWalletByAccountIdAsync(Guid Id)
    {
        var wallet = await _dbContext.Wallets.Include(i=>i.Renter)
            .SingleOrDefaultAsync(a=>a.RenterId==Id);
        return wallet;
    }
}
