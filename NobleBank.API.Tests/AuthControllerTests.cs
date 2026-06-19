using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NobleBank.API.Controllers;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Auth;
using NobleBank.Application.Features.Auth.Commands.ClearSession;
using NobleBank.Application.Features.Auth.Commands.Login;
using NobleBank.Application.Features.Auth.Commands.Register;

namespace NobleBank.API.Tests
{
    public class AuthControllerTests
    {
        // Minimal ITokenService stub — the controller only uses GetUserIdFromToken
        // in the sendBeacon logout path, which is not exercised by these unit tests.
        private static ITokenService StubTokenService() => new FakeTokenService();

        private sealed class FakeTokenService : ITokenService
        {
            public string? UserIdFromToken { get; set; }
            public Guid? SessionIdFromToken { get; set; }

            public Task<string> GenerateToken(string userId, string email, string fullName)
                => Task.FromResult(string.Empty);

            public Task<string> GenerateToken(string userId, string email, string fullName, Guid sessionId)
            {
                throw new NotImplementedException();
            }

            public string? GetUserIdFromToken(string token) => UserIdFromToken;

            public Guid? GetSessionIdFromToken(string token) => SessionIdFromToken;
        }

        [Fact]
        public async Task Register_WhenSuccessful_ShouldReturnOkWithToken()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator(request => request switch
            {
                RegisterCommand => new AuthResult(true, "token-123", null),
                _ => null
            });
            var controller = new AuthController(mediator, StubTokenService());
            var command = new RegisterCommand("john.doe@example.com", "Password123!", "John", "Doe");

            // Act
            IActionResult result = await controller.Register(command);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("token-123", GetPropertyValue<string>(ok.Value, "token"));
        }

        [Fact]
        public async Task Register_WhenFailed_ShouldReturnBadRequest()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator(request => request switch
            {
                RegisterCommand => new AuthResult(false, null, "Email exists"),
                _ => null
            });
            var controller = new AuthController(mediator, StubTokenService());
            var command = new RegisterCommand("john.doe@example.com", "Password123!", "John", "Doe");

            // Act
            IActionResult result = await controller.Register(command);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email exists", GetPropertyValue<string>(badRequest.Value, "error"));
        }

        [Fact]
        public async Task Login_WhenSuccessful_ShouldReturnOkWithToken()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator(request => request switch
            {
                LoginCommand => new AuthResult(true, "token-456", null),
                _ => null
            });
            var controller = new AuthController(mediator, StubTokenService());
            var command = new LoginCommand("john.doe@example.com", "Password123!");

            // Act
            IActionResult result = await controller.Login(command);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("token-456", GetPropertyValue<string>(ok.Value, "token"));
        }

        [Fact]
        public async Task Login_WhenFailed_ShouldReturnUnauthorized()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator(request => request switch
            {
                LoginCommand => new AuthResult(false, null, "Invalid credentials"),
                _ => null
            });
            var controller = new AuthController(mediator, StubTokenService());
            var command = new LoginCommand("john.doe@example.com", "Password123!");

            // Act
            IActionResult result = await controller.Login(command);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid credentials", GetPropertyValue<string>(unauthorized.Value, "error"));
        }

        [Fact]
        public async Task Login_WhenActiveSessionExists_ShouldReturnConflict()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator(request => request switch
            {
                LoginCommand => new AuthResult(false, null, null, HasActiveSession: true),
                _ => null
            });
            var controller = new AuthController(mediator, StubTokenService());
            var command = new LoginCommand("john.doe@example.com", "Password123!");

            // Act
            IActionResult result = await controller.Login(command);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(true, GetPropertyValue<bool>(conflict.Value, "hasActiveSession"));
        }

        [Fact]
        public async Task Logout_WithBodyToken_ShouldForwardUserIdAndSessionIdToCommand()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            ClearSessionCommand? observed = null;
            var mediator = new TestHelpers.RecordingMediator(request =>
            {
                if (request is ClearSessionCommand cmd) observed = cmd;
                return null;
            });
            var tokenService = new FakeTokenService { UserIdFromToken = "user-1", SessionIdFromToken = sessionId };
            var controller = new AuthController(mediator, tokenService);
            controller.ControllerContext = TestHelpers.CreateControllerContext(userId: null);

            // Act
            IActionResult result = await controller.Logout(new LogoutRequest("any-jwt"));

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.NotNull(observed);
            Assert.Equal("user-1", observed!.UserId);
            Assert.Equal(sessionId, observed.SessionId);
        }

        [Fact]
        public async Task Logout_WithUnknownToken_ShouldNotSendCommand()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator(_ => null);
            var tokenService = new FakeTokenService { UserIdFromToken = null, SessionIdFromToken = null };
            var controller = new AuthController(mediator, tokenService);
            controller.ControllerContext = TestHelpers.CreateControllerContext(userId: null);

            // Act
            IActionResult result = await controller.Logout(new LogoutRequest("bogus"));

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.DoesNotContain(mediator.Requests, r => r is ClearSessionCommand);
        }

        private static T? GetPropertyValue<T>(object? instance, string propertyName)
        {
            Assert.NotNull(instance);
            PropertyInfo? property = instance!.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(property);
            return (T?)property!.GetValue(instance);
        }
    }
}
