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
        IRenterRepository GetRenterRepository();

    IVehicleRepository GetVehicleRepository();

    IBranchRepository GetBranchRepository();
    IVehicleModelRepository GetVehicleModelRepository();
    IRentalPricingRepository GetRentalPricingRepository();

    Task<int> SaveChangesAsync();

}
