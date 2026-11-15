using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WhatsAppClone.Api.Interface;
using WhatsAppClone.Api.Managers;
using WhatsAppClone.Api.Models;
using WhatsAppClone.Api.Repositories;
using WhatsAppClone.Api.Services;

namespace WhatsAppClone.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Repositories, Managers, Services ka registration
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Infrastructure
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<ITokenService, TokenService>();

        // Repositories (Proc_class)
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();

        // Managers
        services.AddScoped<IAuthManager, AuthManager>();
        services.AddScoped<IUserManager, UserManager>();

        return services;
    }

    // JWT settings + authentication setup
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key missing or < 32 chars. Set via user-secrets / environment variable.");

        services.Configure<JwtSettings>(config.GetSection("Jwt"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            });

        services.AddAuthorization();
        return services;
    }
}