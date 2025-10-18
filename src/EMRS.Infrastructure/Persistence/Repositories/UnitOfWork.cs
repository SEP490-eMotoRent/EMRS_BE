using EMRS.Application.Abstractions;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Infrastructure.Persistence;
using EMRS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure;

public class UnitOfWork :     IDisposable, IUnitOfWork
{

    private readonly EMRSDbContext _context;

    private  IAccountRepository accountRepository;

    private IMembershipRepository membershipRepository;

    private IRenterRepository renterRepository;

    private IVehicleRepository vehicleRepository;

    private IVehicleModelRepository vehicleModelRepository;

    private IRentalPricingRepository rentalPricingRepository;

    private IBranchRepository branchRepository;
    public UnitOfWork(EMRSDbContext context,
        IBranchRepository branchRepository,
        IVehicleRepository vehicleRepository,
        IMembershipRepository membershipRepository,
        IAccountRepository accountRepository,
        IRenterRepository renterRepository,
        IVehicleModelRepository vehicleModelRepository,
        IRentalPricingRepository rentalPricingRepository)
    {
        _context = context;
        this.branchRepository = branchRepository;
        this.vehicleModelRepository = vehicleModelRepository;
        this.vehicleRepository = vehicleRepository;
        this.accountRepository = accountRepository;
        this.membershipRepository = membershipRepository;
        this.renterRepository = renterRepository;
        this.rentalPricingRepository = rentalPricingRepository;
    }
    public IRentalPricingRepository GetRentalPricingRepository() => rentalPricingRepository;
    public IAccountRepository GetAccountRepository() => accountRepository;
    public IBranchRepository GetBranchRepository() => branchRepository; 
    public IVehicleModelRepository GetVehicleModelRepository() => vehicleModelRepository;   
    public IVehicleRepository GetVehicleRepository() => vehicleRepository;
    public IRenterRepository GetRenterRepository() => renterRepository;
    public IMembershipRepository GetMembershipRepository() => membershipRepository;

   
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

}