using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IWalletRepository
{
    void Add(Wallet entity);

    Task AddAsync(Wallet entity);

    void Delete(Wallet entity);

    Task<Wallet?> GetWalletWithNoUserAsync();
    IEnumerable<Wallet> GetAll();

    Task<List<Wallet>> GetAllAsync();

    Wallet? FindById(Guid id);

    Task<Wallet?> FindByIdAsync(Guid id);

    Task<Wallet?> GetWalletByAccountIdAsync(Guid Id);

    void Update(Wallet entity);


    IQueryable<Wallet> Query();

    Task<bool> IsEmptyAsync();
}
