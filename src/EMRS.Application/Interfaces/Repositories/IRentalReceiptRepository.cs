using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IRentalReceiptRepository
{
    void Add(RentalReceipt entity);

    Task AddAsync(RentalReceipt entity);

    void Delete(RentalReceipt entity);


    IEnumerable<RentalReceipt> GetAll();

    Task<List<RentalReceipt>> GetAllAsync();

    RentalReceipt? FindById(Guid id);

    Task<RentalReceipt?> FindByIdAsync(Guid id);

    Task<RentalReceipt> GetRentalReceiptByBookingId(Guid bookingId);
    void Update(RentalReceipt entity);
    Task<RentalReceipt?> GetRentalReceiptWithReferences(Guid rentalReceiptId);

    IQueryable<RentalReceipt> Query();

    Task<bool> IsEmptyAsync();
}
