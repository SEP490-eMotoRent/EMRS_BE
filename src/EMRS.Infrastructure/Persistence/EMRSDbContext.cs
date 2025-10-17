using EMRS.Application.Abstractions;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence;
 public class EMRSDbContext : DbContext, IAppDbContext
{
    public EMRSDbContext(DbContextOptions<EMRSDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AdditionalFee> AdditionalFees { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<ChargingRecord> ChargingRecords { get; set; }
    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<HolidayPricing> HolidayPricings { get; set; }
    public DbSet<InsuranceClaim> InsuranceClaims { get; set; }
    public DbSet<InsurancePackage> InsurancePackages { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<RentalContract> RentalContracts { get; set; }
    public DbSet<RentalPricing> RentalPricings { get; set; }
    public DbSet<RentalReceipt> RentalReceipts { get; set; }
    public DbSet<Renter> Renters { get; set; }
    public DbSet<RepairRequest> RepairRequests { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleModel> VehicleModels { get; set; }
    public DbSet<VehicleTransferOrder> VehicleTransferOrders { get; set; }
    public DbSet<VehicleTransferRequest> VehicleTransferRequests { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        

    }

    async Task IAppDbContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
         await  SaveChangesAsync(cancellationToken);
    }
}
