using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
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
}
