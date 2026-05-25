using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NobleBank.Domain.Entities;
using NobleBank.Domain.Interfaces;
using NobleBank.Infrastructure.Identity;
using NobleBank.Infrastructure.Persistence;
using NobleBank.Infrastructure.Services;
using NobleBank.Infrastructure.Settings;

namespace NobleBank.Infrastructure.Tests
{
    internal static class TestHelpers
    {
        public static AesEncryptionService CreateEncryptionService()
        {
            var settings = Options.Create(new EncryptionSettings
            {
                Key = Convert.ToBase64String(new byte[32]),
                IV = Convert.ToBase64String(new byte[16])
            });

            return new AesEncryptionService(settings);
        }

        public static TokenService CreateTokenService(string secret = "super-secret-key-1234567890123456")
        {
            var settings = Options.Create(new JwtSettings
            {
                Secret = secret,
                Issuer = "issuer",
                Audience = "audience",
                ExpiryMinutes = 60
            });

            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());
            userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { "User" });

            return new TokenService(settings, userManagerMock.Object);
        }

        public static TokenService CreateTokenServiceWithRoles(params string[] roles)
        {
            var settings = Options.Create(new JwtSettings
            {
                Secret = "super-secret-key-1234567890123456",
                Issuer = "issuer",
                Audience = "audience",
                ExpiryMinutes = 60
            });

            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());
            userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(roles.ToList());

            return new TokenService(settings, userManagerMock.Object);
        }

        public static ApplicationDbContext CreateDbContext(IEncryptionService encryption, MediatR.IMediator? mediator = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options, mediator ?? new FakeMediator(), encryption);
        }

        private sealed class FakeMediator : MediatR.IMediator
        {
            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : MediatR.INotification => Task.CompletedTask;
            public Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : MediatR.IRequest => throw new NotImplementedException();
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(MediatR.IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }
    }
}
