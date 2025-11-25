using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using HandlebarsDotNet;
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
        return await  eMRSDbContext.Documents.Where(a => a.RenterId == renterID&&a.IsDeleted== false
        ).ToListAsync();
    }
    public async Task<Document?> GetDocumentWithReferenceForModifyAsync(Guid documentID)
    {
        return await eMRSDbContext.Documents.Include(a=>a.Renter)
            .FirstOrDefaultAsync(a => a.Id == documentID&&a.IsDeleted==false);
    }
    public async Task<bool> HasBothDocumentImagesAsync(Guid renterId)
    {
        var renterDocs = await eMRSDbContext.Documents
            .AsNoTracking()
            .Where(d => d.RenterId == renterId &&
            d.IsDeleted == false &&
                        (d.DocumentType == DocumentTypeEnum.Driving.ToString() ||
                         d.DocumentType == DocumentTypeEnum.Citizen.ToString()))
            .Select(d => new { d.Id, d.DocumentType })
            .ToListAsync();

        bool hasDriving = renterDocs.Any(d => d.DocumentType == DocumentTypeEnum.Driving.ToString() &&
                                               eMRSDbContext.Media.Any(m => m.DocNo == d.Id));
        bool hasCitizen = renterDocs.Any(d => d.DocumentType == DocumentTypeEnum.Citizen.ToString() &&
                                               eMRSDbContext.Media.Any(m => m.DocNo == d.Id));

        return hasDriving && hasCitizen;
    }



}
