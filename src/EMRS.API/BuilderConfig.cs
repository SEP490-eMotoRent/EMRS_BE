using API.Middlewares;
using EMRS.Application;
using EMRS.Infrastructure;

namespace EMRS.API;

    public static class BuilderConfig
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddApplication(configuration);


        // Signing exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}
