using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IAccountRepository
{
    Task AddAsync(Account entity);
    Task<List<Account>> GetAllAsync();

    Task<Account?> LoginAsync(string username);
    Task<bool> GetByEmaiAndUsernameAsync(string email, string username);
    void Delete(Account entity);

    Task<IEnumerable<Account>> GetAccountsWithReferenceAsync();
    IEnumerable<Account> GetAll();


    Account? FindById(Guid id);

    Task<Account?> FindByIdAsync(Guid id);



    void Update(Account entity);


    IQueryable<Account> Query();

    Task<bool> IsEmptyAsync();
}
