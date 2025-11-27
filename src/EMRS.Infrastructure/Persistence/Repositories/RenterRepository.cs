using EMRS.Application.Abstractions.Models.FacePlusPlus;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class RenterRepository:GenericRepository<Renter>,IRenterRepository
{
    private readonly EMRSDbContext _context;
    public RenterRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<Renter> GetRenterByRenterIdAsync(Guid renterId)
    {
        return await _context.Renters.Include(n=>n.Account)
            .Include(n=>n.Membership)
            .SingleOrDefaultAsync(r => r.Id == renterId);
    }
    public async Task<Renter?> GetRenterByCitizenAsync(string citizenId)
    {
        return await Query()
            .Include(r => r.Account)
            .Include(r => r.Documents)
            .FirstOrDefaultAsync(r =>
                !r.IsDeleted&& 
                r.Documents.Any(d =>
                    d.DocumentType == DocumentTypeEnum.Citizen.ToString() &&
                    d.DocumentNumber == citizenId));
    }

}
