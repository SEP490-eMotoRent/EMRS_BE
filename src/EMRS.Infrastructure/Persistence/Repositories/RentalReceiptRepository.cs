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
        return await Query().Where(v => v.Id == rentalReceiptId)
            .Include(v => v.Booking).ThenInclude(v => v.RentalContract)
            .SingleOrDefaultAsync();
    }
}
