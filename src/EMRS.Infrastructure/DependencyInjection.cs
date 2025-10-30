using EMRS.Application.Abstractions;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
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
        services.AddScoped<IFptFaceSearchClient,FptFaceSearchClient>();

        services.AddScoped<ITransactionRepository, TransactionRepository>();    
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

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));




        return services;
    }
}

