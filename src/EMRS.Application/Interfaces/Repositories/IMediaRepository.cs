using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IMediaRepository
{
    void Add(Media entity);

    Task AddAsync(Media entity);

    void Delete(Media entity);
    Task<IEnumerable<Media>> GetAllMediasWithTheSameDocnoForModifyAsync(Guid DocNo);
    Task AddRangeAsync(IEnumerable<Media> entity);
    Task<IEnumerable<Media>> GetMediasByEntityIdAsync(Guid entityId);
    void Update(Media entity);
    Task DeleteRangeAsync(IEnumerable<Media> entities);

    Task<Media?> GetAMediaWithCondAsync(Guid entityId, string mediaEntityType);
    IQueryable<Media> Query();
    Media? FindById(Guid id);

    Task<Media?> FindByIdAsync(Guid id);
}
