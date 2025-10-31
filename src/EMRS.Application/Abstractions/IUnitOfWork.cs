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
    IRentalReceiptRepository GetRentalReceiptRepository();
    IAccountRepository GetAccountRepository();
    IMembershipRepository GetMembershipRepository();
    IDocumentRepository GetDocumentRepository();
        IRenterRepository GetRenterRepository();
    IMediaRepository GetMediaRepository();
    IVehicleRepository GetVehicleRepository();
    ITransactionRepository GetTransactionRepository();
    IConfigurationRepository GetConfigurationRepository();
    IBookingRepository GetBookingRepository();
    IBranchRepository GetBranchRepository();
    IVehicleModelRepository GetVehicleModelRepository();
    IRentalPricingRepository GetRentalPricingRepository();
    IWalletRepository GetWalletRepository();
    IRentalContractRepository GetRentalContractRepository();

    IInsurancePackageRepository GetInsurancePackageRepository();

    IInsuranceClaimRepository GetInsuranceClaimRepository();

    IStaffRepository GetStaffRepository();

    Task<int> SaveChangesAsync();

    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
   
}
