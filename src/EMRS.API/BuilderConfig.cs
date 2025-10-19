using API.Middlewares;
using EMRS.Application;
using EMRS.Application.Common;
using EMRS.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace EMRS.API;

    public static class BuilderConfig
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("SECRET_KEY")
                    ?? throw new InvalidOperationException("SECRET_KEY not set");
        var key = Encoding.ASCII.GetBytes(jwtSecret);

        // Thêm authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });
        services.AddHttpContextAccessor(); 
       services.AddInfrastructure(configuration);
        services.AddApplication(configuration);


        // Signing exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddAutoMapper(cfg => { },
    typeof(MappingProfile).Assembly);
        return services;
    }
}
