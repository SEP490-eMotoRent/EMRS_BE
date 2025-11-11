using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IHolidayPricingRepository
    {
        void Add(HolidayPricing entity);

        Task AddAsync(HolidayPricing entity);

        void Delete(HolidayPricing entity);

        Task<HolidayPricing?> GetHolidayByCurrentDateAsync();
        IEnumerable<HolidayPricing> GetAll();
        Task DeleteRangeAsync(IEnumerable<HolidayPricing> entities);
        Task<List<HolidayPricing>> GetAllAsync();

        HolidayPricing? FindById(Guid id);

        Task<HolidayPricing?> FindByIdAsync(Guid id);



        void Update(HolidayPricing entity);


        IQueryable<HolidayPricing> Query();

        Task<bool> IsEmptyAsync();
    }
}
