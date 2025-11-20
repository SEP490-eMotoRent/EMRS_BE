using EMRS.Application.Common;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IRepairRequestRepository
    {
        void Add(RepairRequest entity);

        Task AddAsync(RepairRequest entity);

        void Delete(RepairRequest entity);
        Task<PaginationResult<List<RepairRequest>>> GetAllPaginatedAsync(
    int pageSize, int pageNum, bool orderByDesc);
        Task<PaginationResult<List<RepairRequest>>> GetByTechnicianIdPaginatedAsync(
   Guid technicianId, int pageSize, int pageNum, bool orderByDesc);
        IEnumerable<RepairRequest> GetAll();
        Task DeleteRangeAsync(IEnumerable<RepairRequest> entities);
        Task<List<RepairRequest>> GetAllAsync();

        RepairRequest? FindById(Guid id);

        Task<RepairRequest?> FindByIdAsync(Guid id);



        void Update(RepairRequest entity);


        IQueryable<RepairRequest> Query();

        Task<bool> IsEmptyAsync();
    }
}
