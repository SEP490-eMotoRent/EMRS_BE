using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class RentalReceiptRepository:GenericRepository<RentalReceipt>, IRentalReceiptRepository
{
    private readonly EMRSDbContext _dbContext;
    public RentalReceiptRepository(EMRSDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<RentalReceipt> GetRentalReceiptByBookingId(Guid bookingId)
    {
        return await Query().Where(b=>b.BookingId==bookingId).FirstOrDefaultAsync();
    }
    public async Task<RentalReceipt?> GetRentalReceiptWithReferences(Guid rentalReceiptId)
    {
        return await _dbContext.RentalReceipts.Where(v => v.Id == rentalReceiptId)
            .Include(v => v.Booking)
            .SingleOrDefaultAsync();
    }

    public async Task<RentalReceipt?> GetRentalReceiptWithReferencesAsync(Guid bookingId)
    {
        return await Query()
            .Include(rr => rr.Booking)
                .ThenInclude(b => b.Renter)
                    .ThenInclude(r => r.Account)
            .Include(rr => rr.Booking)
                .ThenInclude(b => b.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
            .Include(rr => rr.Booking)
                .ThenInclude(b => b.ChargingRecords)
            .Include(rr => rr.Booking)
                .ThenInclude(b => b.AdditionalFees)
            .Include(rr => rr.Staff)
                .ThenInclude(s => s.Account)
            .Where(rr => rr.BookingId == bookingId)
            .SingleOrDefaultAsync();
    }
}
