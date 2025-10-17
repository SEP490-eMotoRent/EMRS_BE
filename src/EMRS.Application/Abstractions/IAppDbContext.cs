using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;


namespace EMRS.Application.Abstractions;

//Commute with Infrastructure
public interface IAppDbContext
    {
        DbSet<Account> Accounts { get; set; }
        DbSet<AdditionalFee> AdditionalFees { get; set; }
        DbSet<Booking> Bookings { get; set; }
        DbSet<Branch> Branches { get; set; }
        DbSet<ChargingRecord> ChargingRecords { get; set; }
        DbSet<Configuration> Configurations { get; set; }
        DbSet<Document> Documents { get; set; }
        DbSet<Feedback> Feedbacks { get; set; }
        DbSet<HolidayPricing> HolidayPricings { get; set; }
        DbSet<InsuranceClaim> InsuranceClaims { get; set; }
        DbSet<InsurancePackage> InsurancePackages { get; set; }
        DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
        DbSet<Media> Media { get; set; }
        DbSet<Membership> Memberships { get; set; }
        DbSet<RentalContract> RentalContracts { get; set; }
        DbSet<RentalPricing> RentalPricings { get; set; }
        DbSet<RentalReceipt> RentalReceipts { get; set; }
        DbSet<Renter> Renters { get; set; }
        DbSet<RepairRequest> RepairRequests { get; set; }
        DbSet<Staff> Staffs { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<Vehicle> Vehicles { get; set; }
        DbSet<VehicleModel> VehicleModels { get; set; }
        DbSet<VehicleTransferOrder> VehicleTransferOrders { get; set; }
        DbSet<VehicleTransferRequest> VehicleTransferRequests { get; set; }
        DbSet<Wallet> Wallets { get; set; }
        DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }

