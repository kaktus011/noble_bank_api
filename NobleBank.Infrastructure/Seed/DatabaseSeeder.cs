using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
using NobleBank.Infrastructure.Settings;

namespace NobleBank.Infrastructure.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger("DatabaseSeeder");

            // Seed Roles (always)
            await SeedRolesAsync(roleManager, logger);

            // Seed Admin User (only if configured)
            await SeedAdminUserAsync(serviceProvider, userManager, logger);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            // Seed Roles
            if (!await roleManager.RoleExistsAsync(Roles.Administrator))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Administrator));
                logger.LogInformation("Created role: {Role}", Roles.Administrator);
            }

            if (!await roleManager.RoleExistsAsync(Roles.User))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.User));
                logger.LogInformation("Created role: {Role}", Roles.User);
            }

            // Clean up duplicate roles if any (can happen in in-memory tests)
            foreach (string? roleName in new[] { Roles.Administrator, Roles.User })
            {
                List<IdentityRole> matches = roleManager.Roles.Where(r => r.NormalizedName == roleManager.NormalizeKey(roleName)).ToList();

                if (matches.Count > 1)
                {
                    logger.LogWarning("Found {Count} duplicate roles for {RoleName}. Cleaning up...", matches.Count, roleName);

                    // Keep the first, delete the rest
                    for (int i = 1; i < matches.Count; i++)
                    {
                        await roleManager.DeleteAsync(matches[i]);
                    }
                }
            }
        }

        private static async Task SeedAdminUserAsync(
            IServiceProvider serviceProvider,
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            // Get admin seeder configuration
            IOptions<AdminSeederSettings> adminSeederOptions = serviceProvider.GetRequiredService<IOptions<AdminSeederSettings>>();
            AdminSeederSettings settings = adminSeederOptions.Value;

            // Skip if not configured or explicitly disabled
            if (!settings.IsConfigured)
            {
                logger.LogInformation("Admin user seeding is disabled or not configured. Skipping admin user creation.");

                return;
            }

            string adminEmail = settings.Email!;
            string adminPassword = settings.Password!;

            // Check if admin user already exists
            ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser is null)
            {
                // Create new admin user
                adminUser = new ApplicationUser
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                IdentityResult createResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createResult.Succeeded)
                {
                    string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create admin user {Email}. Errors: {Errors}", adminEmail, errors);

                    return;
                }

                logger.LogInformation("Successfully created admin user: {Email}", adminEmail);
            }
            else
            {
                logger.LogInformation("Admin user {Email} already exists.", adminEmail);
            }

            // Ensure admin user has the Administrator role (regardless of whether just created or pre-existing)
            if (!await userManager.IsInRoleAsync(adminUser, Roles.Administrator))
            {
                IdentityResult addRoleResult = await userManager.AddToRoleAsync(adminUser, Roles.Administrator);

                if (addRoleResult.Succeeded)
                {
                    logger.LogInformation("Added {Email} to {Role} role.", adminEmail, Roles.Administrator);
                }
                else
                {
                    string errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to add {Email} to {Role} role. Errors: {Errors}", adminEmail, Roles.Administrator, errors);
                }
            }
            else
            {
                logger.LogInformation("Admin user {Email} already has {Role} role.", adminEmail, Roles.Administrator);
            }
        }
    }
}
