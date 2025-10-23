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

    Booking? FindById(Guid id);

    Task<Booking?> FindByIdAsync(Guid id);


    Task<IEnumerable<Booking>> GetBookingsByRenterIdAsync(Guid renterId);
    void Update(Booking entity);


    IQueryable<Booking> Query();

    Task<bool> IsEmptyAsync();
}
