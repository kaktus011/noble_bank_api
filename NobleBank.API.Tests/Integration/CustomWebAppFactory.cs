using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NobleBank.Infrastructure.Persistence;
using NobleBank.Infrastructure.Settings;
using System.Security.Claims;

namespace NobleBank.API.Tests.Integration
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var userHeader = Context.Request.Headers["X-Test-User"].ToString();
            var rolesHeader = Context.Request.Headers["X-Test-Roles"].ToString();

            if (string.IsNullOrEmpty(userHeader))
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userHeader) };
            if (!string.IsNullOrEmpty(rolesHeader))
            {
                foreach (var r in rolesHeader.Split(',')) claims.Add(new Claim(ClaimTypes.Role, r.Trim()));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    /// <summary>
    /// Configures test encryption settings to satisfy validation without requiring real secrets.
    /// </summary>
    public class TestEncryptionSettingsConfigurator : IConfigureOptions<EncryptionSettings>
    {
        public void Configure(EncryptionSettings options)
        {
            // This won't work because properties are init-only.
            // Instead, we'll remove and re-add the options below.
        }
    }

    public class CustomWebAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _testKey;
        private readonly string _testIV;

        public CustomWebAppFactory()
        {
            // Generate valid test encryption keys once for the factory lifetime
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();
                _testKey = Convert.ToBase64String(aes.Key);
                _testIV = Convert.ToBase64String(aes.IV);
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Set environment variables for test encryption settings BEFORE building
            builder.UseEnvironment("Test");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Clear existing sources and add in-memory test config
                config.Sources.Clear();
                var inMemoryConfig = new Dictionary<string, string>
                {
                    // Encryption settings
                    { "Encryption:Key", _testKey },
                    { "Encryption:IV", _testIV },

                    // JWT settings (use dummy values for testing)
                    { "Jwt:Secret", "test-secret-key-that-is-long-enough-for-testing-purposes-at-least-32-chars" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:ExpiryMinutes", "60" },

                    // Admin Seeder Settings (provide test credentials for integration tests)
                    { "AdminSeeder:Email", "admin@test.noblebank.com" },
                    { "AdminSeeder:Password", "TestAdmin123!@#" },
                    { "AdminSeeder:Disabled", "false" },

                    // Connection string (not actually used since we use InMemory)
                    { "ConnectionStrings:DefaultConnection", "Server=.;Database=TestDb;Trusted_Connection=true;" }
                };
                config.AddInMemoryCollection(inMemoryConfig);
            });

            builder.ConfigureServices(services =>
            {
                // Replace authentication with test scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                // Replace DB with InMemory for end-to-end happy path

                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbDescriptor != null) services.Remove(dbDescriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));


                using (var serviceProvider = services.BuildServiceProvider())
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        db.Database.EnsureDeleted();
                        db.Database.EnsureCreated();
                    }
                }
            });
        }

        /// <summary>
        /// Resets the in-memory database by deleting and recreating it. 
        /// Call this between test methods if you need isolation.
        /// </summary>
        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
