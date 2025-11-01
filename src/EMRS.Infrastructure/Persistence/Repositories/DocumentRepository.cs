using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class DocumentRepository:GenericRepository<Document>, IDocumentRepository
{
    private readonly EMRSDbContext eMRSDbContext;
    public DocumentRepository(EMRSDbContext context): base(context)
    {
        eMRSDbContext = context;
    }
    public async Task<IEnumerable<Document>> GetDocumentByRenterIdAsync(Guid renterID)
    {
        return await  eMRSDbContext.Documents.Where(a => a.RenterId == renterID
        ).ToListAsync();
    }
    public async Task<Document?> GetDocumentWithReferenceForModifyAsync(Guid documentID)
    {
        return await eMRSDbContext.Documents.Include(a=>a.Renter)
            .FirstOrDefaultAsync(a => a.Id == documentID);
    }
}
