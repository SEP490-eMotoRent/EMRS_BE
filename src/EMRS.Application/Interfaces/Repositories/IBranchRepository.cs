using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IBranchRepository
{
    void Add(Branch entity);

    Task AddAsync(Branch entity);

    void Delete(Branch entity);


    IEnumerable<Branch> GetAll();

    Task<List<Branch>> GetAllAsync();

    Branch? FindById(Guid id);

    Task<Branch?> FindByIdAsync(Guid id);



    void Update(Branch entity);


    IQueryable<Branch> Query();
}
