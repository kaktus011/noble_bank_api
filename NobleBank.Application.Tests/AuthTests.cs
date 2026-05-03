using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Auth;
using NobleBank.Application.Features.Auth.Commands.Login;
using NobleBank.Application.Features.Auth.Commands.Register;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Tests
{
    public class AuthTests
    {
        [Fact]
        public void LoginCommandValidator_WithValidData_ShouldPass()
        {
            // Arrange
            var validator = new LoginCommandValidator();
            var command = new LoginCommand("john.doe@example.com", "Password123!");

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void RegisterCommandValidator_WithInvalidData_ShouldReturnErrors()
        {
            // Arrange
            var validator = new RegisterCommandValidator();
            var command = new RegisterCommand("", "weak", "", "");

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(RegisterCommand.Email));
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(RegisterCommand.Password));
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(RegisterCommand.FirstName));
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(RegisterCommand.LastName));
        }

        [Fact]
        public async Task RegisterCommandHandler_WhenRegistrationSucceeds_ShouldReturnToken()
        {
            // Arrange
            var identityService = new FakeIdentityService
            {
                RegisterResult = (true, "user-1", null)
            };
            var tokenService = new FakeTokenService { Token = "jwt-token" };
            var handler = new RegisterCommandHandler(identityService, tokenService);
            var command = new RegisterCommand("john.doe@example.com", "Password123!", "John", "Doe");

            // Act
            AuthResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("jwt-token", result.Token);
            Assert.Null(result.Error);
            Assert.Equal(("user-1", "john.doe@example.com", "John Doe"), tokenService.LastTokenRequest);
        }

        [Fact]
        public async Task RegisterCommandHandler_WhenRegistrationFails_ShouldReturnFailureResult()
        {
            // Arrange
            var identityService = new FakeIdentityService
            {
                RegisterResult = (false, null, "Email already exists")
            };
            var tokenService = new FakeTokenService { Token = "jwt-token" };
            var handler = new RegisterCommandHandler(identityService, tokenService);
            var command = new RegisterCommand("john.doe@example.com", "Password123!", "John", "Doe");

            // Act
            AuthResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("Email already exists", result.Error);
        }

        [Fact]
        public async Task LoginCommandHandler_WhenLoginSucceedsAndUserExists_ShouldReturnTokenWithStoredUserData()
        {
            // Arrange
            var identityService = new FakeIdentityService
            {
                LoginResult = (true, "user-1", null)
            };
            var tokenService = new FakeTokenService { Token = "jwt-token" };
            var context = CreateDbContext();
            context.Users.Add(new ApplicationUser
            {
                Id = "user-1",
                Email = "stored@example.com",
                FirstName = "John",
                LastName = "Doe"
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new LoginCommandHandler(identityService, tokenService, context);
            var command = new LoginCommand("john.doe@example.com", "Password123!");

            // Act
            AuthResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("jwt-token", result.Token);
            Assert.Null(result.Error);
            Assert.Equal(("user-1", "stored@example.com", "John Doe"), tokenService.LastTokenRequest);
        }

        [Fact]
        public async Task LoginCommandHandler_WhenLoginFails_ShouldReturnFailureResult()
        {
            // Arrange
            var identityService = new FakeIdentityService
            {
                LoginResult = (false, null, "Invalid credentials")
            };
            var tokenService = new FakeTokenService { Token = "jwt-token" };
            var context = CreateDbContext();
            var handler = new LoginCommandHandler(identityService, tokenService, context);
            var command = new LoginCommand("john.doe@example.com", "Password123!");

            // Act
            AuthResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("Invalid credentials", result.Error);
        }

        private static TestApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestApplicationDbContext(options);
        }

        private sealed class FakeIdentityService : IIdentityService
        {
            public (bool Success, string? UserId, string? Error) RegisterResult { get; set; }
            public (bool Success, string? UserId, string? Error) LoginResult { get; set; }

            public Task<(bool Success, string UserId, string Error)> RegisterAsync(string email, string password, string firstName, string lastName)
                => Task.FromResult((RegisterResult.Success, RegisterResult.UserId ?? string.Empty, RegisterResult.Error ?? string.Empty));

            public Task<(bool Success, string UserId, string Error)> LoginAsync(string email, string password)
                => Task.FromResult((LoginResult.Success, LoginResult.UserId ?? string.Empty, LoginResult.Error ?? string.Empty));
        }

        private sealed class FakeTokenService : ITokenService
        {
            public string Token { get; set; } = string.Empty;
            public (string UserId, string Email, string FullName)? LastTokenRequest { get; private set; }

            public string GenerateToken(string userId, string email, string fullName)
            {
                LastTokenRequest = (userId, email, fullName);
                return Token;
            }
        }

        private sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
        {
            public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<Card> Cards => Set<Card>();
            public DbSet<Loan> Loans => Set<Loan>();
            public DbSet<Transaction> Transactions => Set<Transaction>();
            public DbSet<Post> Posts => Set<Post>();
            public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
                => base.SaveChangesAsync(cancellationToken);
        }
    }
}
