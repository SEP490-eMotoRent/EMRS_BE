using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.BackgroundJobs.Booking;
using EMRS.Application.Abstractions.Models;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Application.Services;
using EMRS.Domain.Entities;
using EMRS.Infrastructure.BackgroundJobs.Booking;
using EMRS.Infrastructure.Persistence;
using EMRS.Infrastructure.Persistence.Repositories;
using EMRS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure;

    public static  class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,  IConfiguration configuration)
    {
        services.AddDbContext<EMRSDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"))
           .UseSnakeCaseNamingConvention());
        /* services.AddDbContext<EMRSDbContext>(options =>
     options.UseMySql(configuration.GetConnectionString("MySqlConnection"),
     new MySqlServerVersion(new Version(8, 0, 41))));*/
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IQuestPdfGenerator, QuestPdfGenerator>();
        services.AddScoped<IFacePlusPlusService,FacePlusPlusService>();
        services.AddScoped<IVNPayService,VNPayService>();
        services.AddScoped<IProtrackService, ProtrackService>();
        services.AddScoped<IFlespiService, FlespiService>();


        services.AddScoped<ITransactionRepository, TransactionRepository>();    
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IRenterRepository, RenterRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
        services.AddScoped<IRentalPricingRepository, RentalPricingRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IRentalReceiptRepository, RentalReceiptRepository>();
        services.AddScoped<IRentalContractRepository, RentalContractRepository>();
        services.AddScoped<IInsurancePackageRepository, InsurancePackageRepository>();
        services.AddScoped<IInsuranceClaimRepository, InsuranceClaimRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IGeminiAIService, GeminiAIService>();
        services.AddScoped<IAdditionalFeeRepository, AdditionalFeeRepository>();
        services.AddScoped<IChargingRecordRepository, ChargingRecordRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IHolidayPricingRepository, HolidayPricingRepository>();

        services.AddScoped<IVehicleTransferRequestRepository, VehicleTransferRequestRepository>();
        services.AddScoped<IVehicleTransferOrderRepository, VehicleTransferOrderRepository>();

        services.AddScoped<IWithdrawalRequestRepository, WithdrawalRequestRepository>();
        
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        services.AddScoped<IBookingJobScheduler, BookingJobScheduler>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));




        return services;
    }
}

