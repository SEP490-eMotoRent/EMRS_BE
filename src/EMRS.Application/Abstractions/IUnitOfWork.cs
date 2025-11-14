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
    IHolidayPricingRepository GetHolidayPricingRepository();
    IRentalReceiptRepository GetRentalReceiptRepository();
    IAccountRepository GetAccountRepository();
    IMembershipRepository GetMembershipRepository();
    IDocumentRepository GetDocumentRepository();
    IFeedbackRepository GetFeedbackRepository();
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

    IAdditionalFeeRepository GetAdditionalFeeRepository();

    IChargingRecordRepository GetChargingRecordRepository();
    IStaffRepository GetStaffRepository();

    IVehicleTransferRequestRepository GetVehicleTransferRequestRepository();
    IVehicleTransferOrderRepository GetVehicleTransferOrderRepository();

    IWithdrawalRequestRepository GetWithdrawalRequestRepository();
    Task<int> SaveChangesAsync();

    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
   
}
