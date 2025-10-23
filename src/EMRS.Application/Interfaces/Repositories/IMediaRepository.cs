using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IMediaRepository
{
    Task AddRangeAsync(IEnumerable<Media> entity);
    Task<IEnumerable<Media>> GetMediasByEntityIdAsync(Guid entityId);

    IQueryable<Media> Query();
}
