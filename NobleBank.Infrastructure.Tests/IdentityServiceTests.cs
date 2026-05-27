using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
using NobleBank.Infrastructure.Identity;
using Xunit;

namespace NobleBank.Infrastructure.Tests
{
    public class IdentityServiceTests
    {
        [Fact]
        public async Task RegisterAsync_WhenDefaultRoleDoesNotExist_ShouldReturnFailureWithoutCreatingUser()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var userManager = CreateUserManager(userStore);
            var roleManager = CreateRoleManager(roleStore);
            var service = new IdentityService(userManager.Object, roleManager.Object, CreateSignInManager(userStore).Object);

            roleManager.Setup(x => x.RoleExistsAsync(Roles.User)).ReturnsAsync(false);

            // Act
            var result = await service.RegisterAsync("john.doe@example.com", "Password123!", "John", "Doe");

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Default role '{Roles.User}' is not configured.", result.Error);
            userManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            userManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenRoleAssignmentFails_ShouldDeleteCreatedUserAndReturnFailure()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var userManager = CreateUserManager(userStore);
            var roleManager = CreateRoleManager(roleStore);
            var service = new IdentityService(userManager.Object, roleManager.Object, CreateSignInManager(userStore).Object);

            var user = new ApplicationUser { Id = "user-1", Email = "john.doe@example.com", UserName = "john.doe@example.com" };

            roleManager.Setup(x => x.RoleExistsAsync(Roles.User)).ReturnsAsync(true);
            userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.User))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed" }));
            userManager.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await service.RegisterAsync("john.doe@example.com", "Password123!", "John", "Doe");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Role assignment failed", result.Error);
            userManager.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WhenRoleAssignmentFailsAndRollbackDeleteFails_ShouldReturnRollbackError()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var userManager = CreateUserManager(userStore);
            var roleManager = CreateRoleManager(roleStore);
            var service = new IdentityService(userManager.Object, roleManager.Object, CreateSignInManager(userStore).Object);

            roleManager.Setup(x => x.RoleExistsAsync(Roles.User)).ReturnsAsync(true);
            userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.User))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed" }));
            userManager.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failed" }));

            // Act
            var result = await service.RegisterAsync("john.doe@example.com", "Password123!", "John", "Doe");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Role assignment failed. Rollback failed: Delete failed", result.Error);
        }

        private static Mock<UserManager<ApplicationUser>> CreateUserManager(Mock<IUserStore<ApplicationUser>> store)
            => new(store.Object, null, null, null, null, null, null, null, null);

        private static Mock<RoleManager<IdentityRole>> CreateRoleManager(Mock<IRoleStore<IdentityRole>> store)
            => new(store.Object, null, null, null, null);

        private static Mock<SignInManager<ApplicationUser>> CreateSignInManager(Mock<IUserStore<ApplicationUser>> store)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(
                CreateUserManager(store).Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null,
                null,
                null,
                null);
        }
    }
}
