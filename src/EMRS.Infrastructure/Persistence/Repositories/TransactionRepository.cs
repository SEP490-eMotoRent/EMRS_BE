using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class TransactionRepository:GenericRepository<Transaction>, ITransactionRepository
{
    private readonly EMRSDbContext _context;
    public TransactionRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetTransactionsByRenterIdAsync(Guid renterId)
    {

        var bookingIds = await _context.Bookings
            .Where(b => b.RenterId == renterId && !b.IsDeleted)
            .Select(b => b.Id)
            .ToListAsync();


        var wallet = await _context.Wallets
            .Where(w => w.RenterId == renterId && !w.IsDeleted)
            .FirstOrDefaultAsync();

        
        var relevantDocIds = new List<Guid>();

        
        relevantDocIds.AddRange(bookingIds);

        
        if (wallet != null)
        {
            relevantDocIds.Add(wallet.Id);
        }

        //Query Transaction với DocNo trong danh sách relevantDocIds
        return await Query()
            .Where(t => relevantDocIds.Contains(t.DocNo))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

}
