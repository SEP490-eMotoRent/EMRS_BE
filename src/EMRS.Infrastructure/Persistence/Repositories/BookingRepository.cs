using EMRS.Application.Common;
using EMRS.Application.DTOs.BookingDTOs;
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
        return await Query().Include(b=>b.VehicleModel)
            .Include(b=>b.InsurancePackage)
            .Include(b=>b.Renter)
                .ThenInclude(v=>v.Account)
            .Where(Query => Query.RenterId == renterId).ToListAsync();
    }
    public async Task<Booking?> GetBookingByIdWithLessReferencesAsync(Guid bookingId)
    {
        return await Query()
            .Where(b => b.Id == bookingId)
            .Include(b=>b.RentalContract)
            .Include(b=>b.Renter)
            .ThenInclude(r=>r.Account)
            .Include(b=>b.VehicleModel)
            .ThenInclude(z=>z.RentalPricing)
            .Include(b=>b.Vehicle)
            .AsSplitQuery()
            .SingleOrDefaultAsync();
    }
    public async Task<Booking?> GetBookingByIdWithReferencesAsync(Guid bookingId)
    {
        return await Query()
            .Where(b => b.Id == bookingId)
            .Include(b => b.HandoverBranch)
            .Include(b => b.RentalReceipt)
                .ThenInclude(r => r.Staff)
                    .ThenInclude(s => s.Account)
            .Include(b => b.Renter)
                .ThenInclude(r => r.Documents)
            .Include(b => b.Renter.Account)
            .Include(b => b.Vehicle)
                .ThenInclude(v => v.VehicleModel)
                    .ThenInclude(vm => vm.RentalPricing)
            .SingleOrDefaultAsync();
    }


    public async Task<PaginationResult<List<Booking>>> GetBookingWithFilter(BookingSearchRequest bookingSearchRequest,int PageSize, int PageNum)
    {
        if (PageNum <= 0) PageNum = 1;
        if (PageSize <= 0) PageSize = 1;
        var searchResult= Query()
            .Include(b => b.Renter)
                .ThenInclude(r=>r.Account)
            .Include(b=>b.VehicleModel)
            .Include(b => b.Vehicle)
                .ThenInclude(r => r.VehicleModel)
                .ThenInclude(v=>v.RentalPricing)
            .AsSplitQuery()
            .Where(b=>
       (string.IsNullOrEmpty(bookingSearchRequest.RenterId.ToString())  || b.RenterId == bookingSearchRequest.RenterId) &&
         (string.IsNullOrEmpty(bookingSearchRequest.VehicleModelId.ToString())  || b.VehicleModelId == bookingSearchRequest.VehicleModelId) &&
            (string.IsNullOrEmpty(bookingSearchRequest.BookingStatus) || b.BookingStatus == bookingSearchRequest.BookingStatus)
            )
            ;
        var totalCount =  await searchResult.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);
        searchResult = searchResult.Skip((PageNum - 1) * PageSize).Take(PageSize);
        var PaginationResult = new PaginationResult<List<Booking>>
        {
            CurrentPage = PageNum,
            PageSize = PageSize,
            TotalPages = totalPages,
            Items = await searchResult.ToListAsync(),
            TotalItems = totalCount
        };
        return PaginationResult ?? new PaginationResult<List<Booking>>();
    }
}
