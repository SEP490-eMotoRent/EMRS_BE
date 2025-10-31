using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IConfigurationRepository
{
    void Add(Configuration entity);

    Task AddAsync(Configuration entity);

    void Delete(Configuration entity);


    IEnumerable<Configuration> GetAll();
    Task DeleteRangeAsync(IEnumerable<Configuration> entities);
    Task<List<Configuration>> GetAllAsync();

    Configuration? FindById(Guid id);

    Task<Configuration?> FindByIdAsync(Guid id);



    void Update(Configuration entity);


    IQueryable<Configuration> Query();

    Task<bool> IsEmptyAsync();
}
