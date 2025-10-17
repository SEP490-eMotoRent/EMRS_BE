using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    void Add(T entity);

    Task AddAsync(T entity);

    void Delete(T entity);


    IEnumerable<T> GetAll();

    Task<List<T>> GetAllAsync();

    T? FindById(Guid id);

    Task<T?> FindByIdAsync(Guid id);



    void Update(T entity);


    IQueryable<T> Query();
    
}
