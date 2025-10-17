using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface  IAccountRepository
{
    Task AddAsync(Account entity);
    Task<List<Account>> GetAllAsync();

    Task<Account?> GetByEmaiAsync(string email);
}
