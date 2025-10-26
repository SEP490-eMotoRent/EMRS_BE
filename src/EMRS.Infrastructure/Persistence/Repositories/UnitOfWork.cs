using EMRS.Application.Abstractions;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Infrastructure.Persistence;
using EMRS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure;

public class UnitOfWork :     IDisposable, IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    private readonly EMRSDbContext _context;
    private IRentalReceiptRepository rentalReceiptRepository;


    private ITransactionRepository transactionRepository;

    private  IAccountRepository accountRepository;

    private IMembershipRepository membershipRepository;

    private IRenterRepository renterRepository;

    private IVehicleRepository vehicleRepository;

    private IVehicleModelRepository vehicleModelRepository;
    private IMediaRepository mediaRepository;
    private IRentalPricingRepository rentalPricingRepository;
    private IStaffRepository staffRepository;
    private IBranchRepository branchRepository;
    private IBookingRepository bookingRepository;
    private IWalletRepository walletRepository;
    public UnitOfWork(EMRSDbContext context,
        ITransactionRepository transactionRepository,
        IStaffRepository staffRepository,
        IBookingRepository bookingRepository,
        IBranchRepository branchRepository,
        IVehicleRepository vehicleRepository,
        IMembershipRepository membershipRepository,
        IAccountRepository accountRepository,
        IRenterRepository renterRepository,
        IVehicleModelRepository vehicleModelRepository,
        IWalletRepository walletRepository,
        IMediaRepository mediaRepository,
        IRentalPricingRepository rentalPricingRepository,
        IRentalReceiptRepository rentalReceiptRepository)
    {
        _context = context;
        this.transactionRepository = transactionRepository;
        this.mediaRepository = mediaRepository;
        this.staffRepository = staffRepository;
        this.walletRepository = walletRepository;
        this.bookingRepository = bookingRepository;
        this.branchRepository = branchRepository;
        this.vehicleModelRepository = vehicleModelRepository;
        this.vehicleRepository = vehicleRepository;
        this.accountRepository = accountRepository;
        this.membershipRepository = membershipRepository;
        this.renterRepository = renterRepository;
        this.rentalPricingRepository = rentalPricingRepository;
        this.rentalReceiptRepository = rentalReceiptRepository;
    }
    public IRentalReceiptRepository GetRentalReceiptRepository() => rentalReceiptRepository;
    public ITransactionRepository GetTransactionRepository() => transactionRepository;
    public IMediaRepository GetMediaRepository() => mediaRepository;
    public IWalletRepository GetWalletRepository() => walletRepository;
    public IBookingRepository GetBookingRepository() => bookingRepository;  
    public IRentalPricingRepository GetRentalPricingRepository() => rentalPricingRepository;
    public IAccountRepository GetAccountRepository() => accountRepository;
    public IBranchRepository GetBranchRepository() => branchRepository; 
    public IVehicleModelRepository GetVehicleModelRepository() => vehicleModelRepository;   
    public IVehicleRepository GetVehicleRepository() => vehicleRepository;
    public IRenterRepository GetRenterRepository() => renterRepository;
    public IMembershipRepository GetMembershipRepository() => membershipRepository;

    public IStaffRepository GetStaffRepository() => staffRepository;

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    //start transaction for transfer ex: money
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    //commit transaction success ex: wallet
    public async Task CommitAsync()
    {
        
        await _transaction?.CommitAsync()!;
    }
    // transaction failed ex: wallet
    public async Task RollbackAsync()
    {
        await _transaction?.RollbackAsync()!;
    }

}