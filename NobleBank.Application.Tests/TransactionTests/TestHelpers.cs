using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Transactions;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Tests.TransactionTests
{
    internal static class TestHelpers
    {
        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<TransactionMappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
            return configuration.CreateMapper();
        }

        public static TestApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestApplicationDbContext(options);
        }

        public static ApplicationUser CreateUser(string userId, string firstName = "John", string lastName = "Doe", string email = "john.doe@example.com")
        {
            return new ApplicationUser
            {
                Id = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
        }

        public static Card CreateAndActivateCard(
            IApplicationDbContext context,
            string userId,
            CardEnum.Type type = CardEnum.Type.Debit,
            decimal balance = 1000m,
            decimal? creditLimit = null)
        {
            var card = Card.Create(
                cardHolder: "Test User",
                plainCardNumber: "4242424242424242",
                type: type,
                brand: CardEnum.Brand.Visa,
                userId: userId,
                createdBy: userId,
                initialBalance: balance,
                creditLimit: creditLimit);

            card.Activate(userId);
            context.Cards.Add(card);
            return card;
        }

        public static Transaction CreateTransaction(
            Guid id,
            Card card,
            decimal amount,
            string description,
            TransactionsEnum.Type type,
            DateTime occurredAt,
            string performedBy = "user-1")
        {
            var transaction = Transaction.Create(
                amount: amount,
                description: description,
                type: type,
                card: card,
                performedBy: performedBy);

            SetPrivateProperty(transaction, nameof(Transaction.Id), id);
            SetPrivateProperty(transaction, nameof(Transaction.OccurredAt), occurredAt);

            return transaction;
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
        }
    }
}
