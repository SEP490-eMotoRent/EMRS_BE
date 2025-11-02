using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IAdditionalFeeRepository
    {
        void Add(AdditionalFee entity);
        Task AddAsync(AdditionalFee entity);
        void Delete(AdditionalFee entity);
        IEnumerable<AdditionalFee> GetAll();
        Task<List<AdditionalFee>> GetAllAsync();
        AdditionalFee? FindById(Guid id);
        Task<AdditionalFee?> FindByIdAsync(Guid id);
        void Update(AdditionalFee entity);
        IQueryable<AdditionalFee> Query();
        Task<bool> IsEmptyAsync();

        Task<List<AdditionalFee>> GetAdditionalFeesByBookingIdAsync(Guid bookingId);
    }

}
