using EMRS.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Org.BouncyCastle.Crypto.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public  class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly EMRSDbContext DbContext;

    protected GenericRepository(EMRSDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual void Add(T entity)
    {
        DbContext.Set<T>().Add(entity);
    }
    public virtual async Task AddAsync(T entity)
    {
        await DbContext.Set<T>().AddAsync(entity);
    }
    public virtual async Task AddRangeAsync(IEnumerable<T> entity)
    {
        await DbContext.Set<T>().AddRangeAsync(entity);
    }
    public virtual void Delete(T entity)
    {
        var entry = DbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
            DbContext.Set<T>().Attach(entity);
        DbContext.Set<T>().Remove(entity);
    }

    public virtual IEnumerable<T> GetAll()
    {
        
        return DbContext.Set<T>().ToList();
    }
    public virtual async Task<List<T>> GetAllAsync()
    {
        return await DbContext.Set<T>().AsNoTracking().ToListAsync();
    }
    public virtual T? FindById(Guid id)
    {
        return DbContext.Set<T>().Find(id);
    }
    public virtual async Task<T?> FindByIdAsync(Guid id)
    {
        return await DbContext.Set<T>().FindAsync(id);
    }


    public virtual void Update(T entity)
    {
        DbContext.ChangeTracker.Clear();
        DbContext.Set<T>().Update(entity);
    }
   
    public virtual IQueryable<T> Query()
    {
        return DbContext.Set<T>().AsQueryable().AsNoTracking();
    }
    public virtual async Task<bool> IsEmptyAsync()
    {
        return !await DbContext.Set<T>().AsNoTracking().AnyAsync();
    }


}
