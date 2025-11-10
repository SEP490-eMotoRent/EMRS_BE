using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IFeedbackRepository
{
    void Add(Feedback entity);

    Task AddAsync(Feedback entity);

    void Delete(Feedback entity);

    IEnumerable<Feedback> GetAll();
    Task DeleteRangeAsync(IEnumerable<Feedback> entities);
    Task<List<Feedback>> GetAllAsync();

    Feedback? FindById(Guid id);

    Task<Feedback?> FindByIdAsync(Guid id);



    void Update(Feedback entity);


    IQueryable<Feedback> Query();

    Task<bool> IsEmptyAsync();
}
