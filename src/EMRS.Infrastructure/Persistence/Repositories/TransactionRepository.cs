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
            .AsNoTracking()
            .Where(b => b.RenterId == renterId && !b.IsDeleted)
            .Select(b => b.Id)
            .ToListAsync();


        var wallet = await _context.Wallets
            .AsNoTracking()
            .Where(w => w.RenterId == renterId && !w.IsDeleted)
            .FirstOrDefaultAsync();

        var insurnaces= await _context.InsuranceClaims
            .AsNoTracking()
            .Where(i => i.RenterId == renterId && !i.IsDeleted)
            .Select(i => i.Id)
            .ToListAsync();

        var relevantDocIds = new List<Guid>();

        
        relevantDocIds.AddRange(bookingIds);
        relevantDocIds.AddRange(insurnaces);

        if (wallet != null)
        {
            relevantDocIds.Add(wallet.Id);
        }

        return await Query()
            .Where(t => relevantDocIds.Contains(t.DocNo))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<Transaction>> GetTransactionsByVietnamYearAsync(int year)
    {
        var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        var vnFrom = new DateTime(year, 1, 1);
        var vnTo = vnFrom.AddYears(1);

        var utcFrom = TimeZoneInfo.ConvertTimeToUtc(vnFrom, vnTz);
        var utcTo = TimeZoneInfo.ConvertTimeToUtc(vnTo, vnTz);

        return await Query()
            .Where(t =>
                !t.IsDeleted
                && t.CreatedAt >= utcFrom
                && t.CreatedAt < utcTo
            )
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<Transaction>> GetTransactionsByVietnamMonthAsync(int year, int month)
    {
        var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var vnFrom = new DateTime(year, month, 1);
        var vnTo = vnFrom.AddMonths(1);

        var utcFrom = TimeZoneInfo.ConvertTimeToUtc(vnFrom,vnTz);
        var utcTo = TimeZoneInfo.ConvertTimeToUtc(vnTo,vnTz);

        return await Query()
            .Where(t =>
                !t.IsDeleted
                && t.CreatedAt >= utcFrom
                && t.CreatedAt < utcTo
            )
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<Transaction>> GetTransactionsByVietnamDayAsync(DateOnly dateVn)
    {
        var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        var vnFrom = dateVn.ToDateTime(TimeOnly.MinValue);

        var vnTo = dateVn.AddDays(1)
                         .ToDateTime(TimeOnly.MinValue);

        var utcFrom = TimeZoneInfo.ConvertTimeToUtc(vnFrom, vnTz);
        var utcTo = TimeZoneInfo.ConvertTimeToUtc(vnTo, vnTz);

        return await Query()
            .Where(t =>
                !t.IsDeleted
                && t.CreatedAt >= utcFrom
                && t.CreatedAt < utcTo
            )
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByVietnamRangeAsync(
     DateOnly fromDateVn,
     DateOnly toDateVn)
    {
        var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        var vnFrom = fromDateVn.ToDateTime(TimeOnly.MinValue);

        var vnToExclusive = toDateVn.AddDays(1)
                                    .ToDateTime(TimeOnly.MinValue);

        var utcFrom = TimeZoneInfo.ConvertTimeToUtc(vnFrom,vnTz);
        var utcTo = TimeZoneInfo.ConvertTimeToUtc(vnToExclusive,vnTz);

        return await Query()
            .Where(t =>
                !t.IsDeleted
                && t.CreatedAt >= utcFrom
                && t.CreatedAt < utcTo
            )
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }



}
