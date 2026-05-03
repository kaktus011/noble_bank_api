using Microsoft.EntityFrameworkCore;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
using NobleBank.Infrastructure.Persistence;

namespace NobleBank.Infrastructure.Tests
{
    public class ApplicationDbContextTests
    {
        [Fact]
        public async Task SaveChangesAsync_WhenCardIsAdded_ShouldSetAuditFields()
        {
            // Arrange
            var encryption = TestHelpers.CreateEncryptionService();
            await using var context = TestHelpers.CreateDbContext(encryption);

            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Credit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 0m,
                creditLimit: 500m);

            context.Cards.Add(card);

            // Act
            await context.SaveChangesAsync(CancellationToken.None);

            // Assert
            Assert.NotEqual(default, card.CreatedAt);
            Assert.NotEqual(default, card.UpdatedAt);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldEncryptCardNumberWhenPersisted()
        {
            // Arrange
            var encryption = TestHelpers.CreateEncryptionService();
            await using var context = TestHelpers.CreateDbContext(encryption);

            context.Users.Add(new ApplicationUser
            {
                Id = "user-1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            });

            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Credit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 0m,
                creditLimit: 500m);

            context.Cards.Add(card);

            // Act
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();
            var persisted = await context.Cards.FirstAsync(c => c.Id == card.Id, CancellationToken.None);

            // Assert
            Assert.Equal("4242", persisted.Last4Digits);
            Assert.Equal("JOHN DOE", persisted.CardHolder);
            Assert.Equal(card.CardNumber, persisted.CardNumber);
        }
    }
}
