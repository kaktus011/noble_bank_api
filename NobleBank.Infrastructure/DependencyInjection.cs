using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Entities;
using NobleBank.Domain.Interfaces;
using NobleBank.Infrastructure.Identity;
using NobleBank.Infrastructure.Persistence;
using NobleBank.Infrastructure.Services;
using NobleBank.Infrastructure.Settings;

namespace NobleBank.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
     this IServiceCollection services,
     IConfiguration configuration)
        {
            // Encryption
            services.AddOptions<EncryptionSettings>()
                .Bind(configuration.GetSection(EncryptionSettings.SectionName))
                .Validate(s => !string.IsNullOrEmpty(s.Key) && !string.IsNullOrEmpty(s.IV),
                    "Encryption Key and IV must be configured.")
                .ValidateOnStart();

            services.AddSingleton<IEncryptionService, AesEncryptionService>();

            // JWT Settings
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .Validate(s => !string.IsNullOrEmpty(s.Secret),
                    "JWT Secret must be configured.")
                .ValidateOnStart();

            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(
                provider => provider.GetRequiredService<ApplicationDbContext>());

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Identity + Token Services
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
