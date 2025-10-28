using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IRentalContractRepository
{
    void Add(RentalContract entity);

    Task AddAsync(RentalContract entity);

    void Delete(RentalContract entity);


    IEnumerable<RentalContract> GetAll();

    Task<List<RentalContract>> GetAllAsync();

    RentalContract? FindById(Guid id);

    Task<RentalContract?> FindByIdAsync(Guid id);



    void Update(RentalContract entity);


    IQueryable<RentalContract> Query();

    Task<bool> IsEmptyAsync();

}
