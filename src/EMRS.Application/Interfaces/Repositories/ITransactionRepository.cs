using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EMRS.Application.Interfaces.Repositories;

public interface ITransactionRepository
{
    void Add(Transaction entity);

    Task AddAsync(Transaction entity);

    void Delete(Transaction entity);


    IEnumerable<Transaction> GetAll();

    Task<List<Transaction>> GetAllAsync();

    Transaction? FindById(Guid id);

    Task<Transaction?> FindByIdAsync(Guid id);



    void Update(Transaction entity);


    IQueryable<Transaction> Query();

    Task<bool> IsEmptyAsync();
}
