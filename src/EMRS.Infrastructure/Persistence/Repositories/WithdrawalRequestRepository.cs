// EMRS.Infrastructure/Persistence/Repositories/WithdrawalRequestRepository.cs
using EMRS.Application.Common;
using EMRS.Application.DTOs.WithdrawalRequestDTOs;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class WithdrawalRequestRepository : GenericRepository<WithdrawalRequest>, IWithdrawalRequestRepository
{
    private readonly EMRSDbContext _dbContext;

    public WithdrawalRequestRepository(EMRSDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WithdrawalRequest?> GetWithdrawalRequestWithDetailsAsync(Guid id)
    {
        return await Query()
            .Include(wr => wr.Wallet)
                .ThenInclude(w => w.Renter)
                    .ThenInclude(r => r.Account)
            .Where(wr => wr.Id == id)
            .SingleOrDefaultAsync();
    }

    public async Task<List<WithdrawalRequest>> GetWithdrawalRequestsByWalletIdAsync(Guid walletId)
    {
        return await Query()
            .Where(wr => wr.WalletId == walletId)
            .OrderByDescending(wr => wr.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<WithdrawalRequest>> GetWithdrawalRequestsByStatusAsync(string status)
    {
        return await Query()
            .Include(wr => wr.Wallet)
                .ThenInclude(w => w.Renter)
                    .ThenInclude(r => r.Account)
            .Where(wr => wr.Status == status)
            .OrderByDescending(wr => wr.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaginationResult<List<WithdrawalRequest>>> GetWithdrawalRequestsWithFilter(
        WithdrawalRequestSearchRequest request, int pageSize, int pageNum)
    {
        if (pageNum <= 0) pageNum = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = Query()
            .Include(wr => wr.Wallet)
                .ThenInclude(w => w.Renter)
                    .ThenInclude(r => r.Account)
            .Where(wr =>
                (string.IsNullOrEmpty(request.Status) || wr.Status == request.Status) &&
                (request.WalletId == null || wr.WalletId == request.WalletId) &&
                (request.FromDate == null || wr.CreatedAt >= request.FromDate) &&
                (request.ToDate == null || wr.CreatedAt <= request.ToDate));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(wr => wr.CreatedAt)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<List<WithdrawalRequest>>
        {
            TotalItems = totalCount,
            TotalPages = totalPages,
            CurrentPage = pageNum,
            PageSize = pageSize,
            Items = items
        };
    }
}