using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class MediaRepository:GenericRepository<Media>, IMediaRepository
{
    private readonly EMRSDbContext _context;
    public MediaRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Media>> GetMediasByEntityIdAsync(Guid entityId)
    {
        return await Query()
            .Where(m => m.DocNo == entityId)
            .ToListAsync();
    }
    public async Task<IEnumerable<Media>> GetAllMediasWithTheSameDocnoForModifyAsync(Guid DocNo)
    {
        return await _context.Media
            .Where(m => m.DocNo == DocNo)
            .ToListAsync();
    }
    public async Task<Media?> GetAMediaWithCondAsync(Guid entityId,string mediaEntityType)
    {
        return await Query()
            .Where(m => m.EntityType == mediaEntityType)
            .Where(m => m.DocNo == entityId)
            .SingleOrDefaultAsync();
    }
}
