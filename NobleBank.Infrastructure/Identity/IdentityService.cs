using Microsoft.AspNetCore.Identity;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Success, string UserId, string Error)> RegisterAsync(
            string email, string password, string firstName, string lastName)
        {
            ApplicationUser? existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser is not null)
            {
                return (false, string.Empty, Constants.Exceptions.EmailAlreadyRegistered);
            }

            if (!await _roleManager.RoleExistsAsync(Roles.User))
            {
                return (false, string.Empty, $"Default role '{Roles.User}' is not configured.");
            }

            ApplicationUser user = new()
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName
            };

            IdentityResult createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));

                return (false, string.Empty, errors);
            }

            IdentityResult roleResult = await _userManager.AddToRoleAsync(user, Roles.User);

            if (!roleResult.Succeeded)
            {
                IdentityResult deleteResult = await _userManager.DeleteAsync(user);

                string roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));

                if (!deleteResult.Succeeded)
                {
                    string deleteErrors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));

                    return (false, string.Empty, $"{roleErrors}. Rollback failed: {deleteErrors}");
                }

                return (false, string.Empty, roleErrors);
            }

            return (true, user.Id, string.Empty);
        }

        public async Task<(bool Success, string UserId, string Error)> LoginAsync(
            string email, string password)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return (false, string.Empty, Constants.Exceptions.InvalidCredentials);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return (false, string.Empty, Constants.Exceptions.AccountLocked);
            }

            if (!result.Succeeded)
            {
                return (false, string.Empty, Constants.Exceptions.InvalidCredentials);
            }

            return (true, user.Id, string.Empty);
        }
    }
}