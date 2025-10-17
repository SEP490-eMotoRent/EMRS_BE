using EMRS.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface IUnitOfWork:IDisposable
{
    IAccountRepository GetAccountRepository();
    IMembershipRepository GetMembershipRepository();
    Task<int> SaveChangesAsync();

}
