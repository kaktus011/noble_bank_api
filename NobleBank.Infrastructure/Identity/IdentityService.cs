using Microsoft.AspNetCore.Identity;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
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

            ApplicationUser user = new()
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName
            };

            IdentityResult result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));

                return (false, string.Empty, errors);
            }
            
            await _userManager.AddToRoleAsync(user, Roles.User);

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