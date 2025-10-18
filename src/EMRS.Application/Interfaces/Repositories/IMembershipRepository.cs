using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IMembershipRepository
{
    Task AddAsync(Membership entity);
    Task<List<Membership>> GetAllAsync();

    Task<Membership?> FindByIdAsync(Guid id);


}
