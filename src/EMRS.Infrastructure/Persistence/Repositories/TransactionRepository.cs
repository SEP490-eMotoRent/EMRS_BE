using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories;

public class TransactionRepository:GenericRepository<Transaction>, ITransactionRepository
{
    private readonly EMRSDbContext _context;
    public TransactionRepository(EMRSDbContext context) : base(context)
    {
        _context = context;
    }

}
