using EMRS.Infrastructure.BackgroundJobs.Booking;
using Hangfire;
using Hangfire.PostgreSql;

namespace EMRS.API.Utils
{
    public static class HangfireConfig
    {
        public static IServiceCollection AddHangfireService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings()
                      .UsePostgreSqlStorage(configuration["CONNECTION_STRING"]);
            });

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 2;
            });

            services.AddScoped<BookingBackgroundJob>();

            return services;
        }

       
    }
}
