using EMRS.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
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

    IBookingRepository GetBookingRepository();
    IBranchRepository GetBranchRepository();
    IVehicleModelRepository GetVehicleModelRepository();
    IRentalPricingRepository GetRentalPricingRepository();
    IWalletRepository GetWalletRepository();
    Task<int> SaveChangesAsync();

    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
   
}
