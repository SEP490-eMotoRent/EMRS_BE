using EMRS.Application.Common;
using EMRS.Application.Interfaces.Services;
using EMRS.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application;

public static class DependencyInjection
{

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IDocumentService, DocumentService>();

        services.AddScoped<IRentalService, RentalService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IInsurancePackageService, InsurancePackageService>();
        return services;
    }

}
