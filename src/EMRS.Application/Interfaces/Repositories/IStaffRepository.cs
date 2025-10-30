using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IStaffRepository
{
    void Add(Staff entity);
    Task AddAsync(Staff entity);
    void Delete(Staff entity);
    IEnumerable<Staff> GetAll();
    Task<List<Staff>> GetAllAsync();
    Staff? FindById(Guid id);
    Task<Staff?> FindByIdAsync(Guid id);
    void Update(Staff entity);
    IQueryable<Staff> Query();
    Task<bool> IsEmptyAsync();
}
