using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IDocumentRepository
{
    void Add(Document entity);

    Task AddAsync(Document entity);

    void Delete(Document entity);

    Task<Document?> GetDocumentByRenterIdAsync(Guid renterID);
    IEnumerable<Document> GetAll();

    Task<List<Document>> GetAllAsync();
    Task<Document?> GetDocumentWithReferenceForModifyAsync(Guid documentID);
    Document? FindById(Guid id);
    Task DeleteRangeAsync(IEnumerable<Document> entities);
    Task<Document?> FindByIdAsync(Guid id);



    void Update(Document entity);


    IQueryable<Document> Query();

    Task<bool> IsEmptyAsync();
}
