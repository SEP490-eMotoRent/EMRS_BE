using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IChargingRecordRepository
    {
        void Add(ChargingRecord entity);
        Task AddAsync(ChargingRecord entity);
        void Delete(ChargingRecord entity);
        IEnumerable<ChargingRecord> GetAll();
        Task<List<ChargingRecord>> GetAllAsync();
        ChargingRecord? FindById(Guid id);
        Task<ChargingRecord?> FindByIdAsync(Guid id);
        void Update(ChargingRecord entity);
        IQueryable<ChargingRecord> Query();
        Task<bool> IsEmptyAsync();

        // Custom methods
        Task<ChargingRecord?> GetLastChargingRecordByBookingIdAsync(Guid bookingId);
        Task<List<ChargingRecord>> GetChargingRecordsByBookingIdAsync(Guid bookingId);
    }
}
