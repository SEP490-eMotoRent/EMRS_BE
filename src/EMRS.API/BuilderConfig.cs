using API.Middlewares;
using EMRS.Application;
using EMRS.Application.Common;
using EMRS.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;  
namespace EMRS.API;

    public static class BuilderConfig
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddApplication(configuration);


        // Signing exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddAutoMapper(cfg => { },
    typeof(MappingProfile).Assembly);
        return services;
    }
}
