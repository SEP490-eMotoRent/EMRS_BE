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
