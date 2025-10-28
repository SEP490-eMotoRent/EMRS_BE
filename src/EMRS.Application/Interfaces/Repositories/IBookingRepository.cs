using EMRS.Application.Common;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    void Add(Booking entity);

    Task AddAsync(Booking entity);

    void Delete(Booking entity);


    IEnumerable<Booking> GetAll();

    Task<List<Booking>> GetAllAsync();
    Task<Booking?> GetBookingByIdWithLessReferencesAsync(Guid bookingId);

    Task<Booking?> GetBookingByIdWithReferencesAsync(Guid BookingId);
    Booking? FindById(Guid id);

    Task<Booking?> FindByIdAsync(Guid id);

    Task<PaginationResult<List<Booking>>> GetBookingWithFilter(BookingSearchRequest bookingSearchRequest, int PageSize, int PageNum);
    Task<IEnumerable<Booking>> GetBookingsByRenterIdAsync(Guid renterId);
    void Update(Booking entity);


    IQueryable<Booking> Query();

    Task<bool> IsEmptyAsync();
}
