using API.Middlewares;
using AutoMapper;
using EMRS.Application;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Infrastructure;
using EMRS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
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
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse(); 
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    var result = ResultResponse<object>.Unauthorized("Unauthorized or missing JWT token");
                    return context.Response.WriteAsJsonAsync(result);
                },
                OnForbidden = context =>
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    var result = ResultResponse<object>.Forbidden("You do not have permission to access this resource");
                    return context.Response.WriteAsJsonAsync(result);
                }
            };
        });
        services.AddHttpContextAccessor(); 
       services.AddInfrastructure(configuration);
        services.AddApplication(configuration);
        services.AddHttpClient<IFacePlusPlusClient, FacePlusPlusClient>(client =>
        {
            client.BaseAddress = new Uri("https://api-us.faceplusplus.com/facepp/v3/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });



        // Signing exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddAutoMapper(cfg => { },
    typeof(MappingProfile).Assembly);
        return services;
    }
}
