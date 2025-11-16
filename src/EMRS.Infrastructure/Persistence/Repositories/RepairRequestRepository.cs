using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories
{
    public class RepairRequestRepository:GenericRepository<RepairRequest>, IRepairRequestRepository
    {
        private readonly EMRSDbContext _context;
        public RepairRequestRepository(EMRSDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
