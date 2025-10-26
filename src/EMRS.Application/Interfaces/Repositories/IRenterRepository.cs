using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IRenterRepository
{
    Task AddAsync(Renter entity);
    Task<List<Renter>> GetAllAsync();
    Task<Renter> GetRenterByAccountIdAsync(Guid accountId);

}