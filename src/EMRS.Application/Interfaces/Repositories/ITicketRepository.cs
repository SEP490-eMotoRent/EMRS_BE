using EMRS.Application.Common;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface ITicketRepository
    {
        void Add(Ticket entity);

        Task AddAsync(Ticket entity);

        void Delete(Ticket entity);
        Task<PaginationResult<List<Ticket>>> GetAllTicketsByBookingIdAsync(Guid BookingId,
        int pageSize, int pageNum, bool orderByDescending = true);
        Task<PaginationResult<List<Ticket>>> GetAllTicketsByStaffIdAsync(Guid StaffId,
         int pageSize, int pageNum, bool orderByDescending = true);
        Task<PaginationResult<List<Ticket>>> GetAllTicketsAsync(
            int pageSize, int pageNum, bool orderByDescending = true);
        IEnumerable<Ticket> GetAll();
        Task DeleteRangeAsync(IEnumerable<Ticket> entities);
        Task<List<Ticket>> GetAllAsync();

        Ticket? FindById(Guid id);

        Task<Ticket?> FindByIdAsync(Guid id);



        void Update(Ticket entity);


        IQueryable<Ticket> Query();

        Task<bool> IsEmptyAsync();
    }
}
