using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IMembershipRepository
{
    void Add(Membership entity);


    void Delete(Membership entity);


    Task DeleteRangeAsync(IEnumerable<Membership> entities);

    Membership? FindById(Guid id);




    void Update(Membership entity);


    IQueryable<Membership> Query();

    Task<bool> IsEmptyAsync();
    Task AddAsync(Membership entity);
    Task<List<Membership>> GetAllAsync();

    Task<Membership?> FindByIdAsync(Guid id);

    Task<Membership?> FindLowestMinBookingMembershipAsync();

}
