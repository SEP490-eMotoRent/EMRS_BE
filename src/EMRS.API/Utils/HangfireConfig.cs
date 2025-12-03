using EMRS.Infrastructure.BackgroundJobs.Booking;
using EMRS.Infrastructure.BackgroundJobs.GPSSharing;
using EMRS.Infrastructure.BackgroundJobs.Transaction;
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
                options.WorkerCount = 6;
            });

            services.AddScoped<BookingBackgroundJob>();
            services.AddScoped<GPSSharingBackgroundJob>();
            services.AddScoped<TransactionBackgroundJob>();

            return services;
        }

       
    }
}
