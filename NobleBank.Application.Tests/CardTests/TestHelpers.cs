using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Cards;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
using NobleBank.Domain.Interfaces;

namespace NobleBank.Application.Tests.CardTests
{
    internal static class TestHelpers
    {
        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<CardMappingProfile>());
            return configuration.CreateMapper();
        }

        public static TestApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestApplicationDbContext(options);
        }

        public static Card CreateCard(
            Guid id,
            string userId,
            string cardHolder,
            string cardNumber,
            CardEnum.Type type,
            CardEnum.Brand brand,
            CardEnum.Status status,
            decimal balance,
            decimal? creditLimit,
            DateTime expiryDate,
            DateTime createdAt)
        {
            var card = Card.Create(
                cardHolder: cardHolder,
                plainCardNumber: cardNumber,
                type: type,
                brand: brand,
                userId: userId,
                createdBy: userId,
                initialBalance: balance,
                creditLimit: creditLimit);

            SetPrivateProperty(card, nameof(Card.Id), id);
            SetPrivateProperty(card, nameof(Card.ExpiryDate), expiryDate);
            SetPrivateProperty(card, nameof(Card.Status), status);
            SetPrivateProperty(card, nameof(Card.CreatedAt), createdAt);
            SetPrivateProperty(card, nameof(Card.UpdatedAt), createdAt);
            SetPrivateProperty(card, nameof(Card.LastModifiedBy), userId);

            return card;
        }

        public static void SetPrivateProperty<T>(T instance, string propertyName, object value)
        {
            var property = typeof(T).GetProperty(propertyName);
            Assert.NotNull(property);
            property!.SetValue(instance, value);
        }

        internal sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
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

        internal sealed class FakeIdentityService : IIdentityService
        {
            public (bool Success, string? UserId, string? Error) RegisterResult { get; set; }
            public (bool Success, string? UserId, string? Error) LoginResult { get; set; }

            public Task<(bool Success, string UserId, string Error)> RegisterAsync(string email, string password, string firstName, string lastName)
                => Task.FromResult((RegisterResult.Success, RegisterResult.UserId ?? string.Empty, RegisterResult.Error ?? string.Empty));

            public Task<(bool Success, string UserId, string Error)> LoginAsync(string email, string password)
                => Task.FromResult((LoginResult.Success, LoginResult.UserId ?? string.Empty, LoginResult.Error ?? string.Empty));
        }

        internal sealed class FakeTokenService : ITokenService
        {
            public string Token { get; set; } = string.Empty;
            public (string UserId, string Email, string FullName)? LastTokenRequest { get; private set; }

            public string GenerateToken(string userId, string email, string fullName)
            {
                LastTokenRequest = (userId, email, fullName);
                return Token;
            }
        }
    }
}
