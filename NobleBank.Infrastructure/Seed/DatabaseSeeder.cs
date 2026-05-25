using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Infrastructure.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed Roles
            if (!await roleManager.RoleExistsAsync(Roles.Administrator))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Administrator));
            }

            if (!await roleManager.RoleExistsAsync(Roles.User))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.User));
            }

            // Seed Admin User
            const string adminEmail = "admin@noblebank.com";
            const string adminPassword = "Admin123!@#";

            ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);

            // Clean up duplicate roles if any (can happen in in-memory tests)
            foreach (var roleName in new[] { Roles.Administrator, Roles.User })
            {
                var matches = roleManager.Roles.Where(r => r.NormalizedName == roleManager.NormalizeKey(roleName)).ToList();
                if (matches.Count > 1)
                {
                    // Keep the first, delete the rest
                    for (int i = 1; i < matches.Count; i++)
                    {
                        await roleManager.DeleteAsync(matches[i]);
                    }
                }
            }

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    if (!await userManager.IsInRoleAsync(adminUser, Roles.Administrator))
                    {
                        await userManager.AddToRoleAsync(adminUser, Roles.Administrator);
                    }
                }
            }
        }
    }
}
