using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EMRS.Application.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetTransactionsByVietnamMonthAsync(int year, int month);
    void Add(Transaction entity);
    Task<List<Transaction>> GetTransactionsByVietnamYearAsync(int year);
    Task AddAsync(Transaction entity);
    Task<List<Transaction>> GetTransactionsByVietnamDayAsync(DateOnly dateVn);
    void Delete(Transaction entity);

    Task<List<Transaction>> GetTransactionsByVietnamRangeAsync(
     DateOnly fromDateVn,
     DateOnly toDateVn);
    IEnumerable<Transaction> GetAll();

    Task<List<Transaction>> GetAllAsync();

    Transaction? FindById(Guid id);

    Task<Transaction?> FindByIdAsync(Guid id);



    void Update(Transaction entity);


    IQueryable<Transaction> Query();

    Task<bool> IsEmptyAsync();

    Task<List<Transaction>> GetTransactionsByRenterIdAsync(Guid renterId);
}
