using NobleBank.Application.Features.Admin.Queries.GetPendingCards;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Application.Tests.CardTests;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.AdminTests
{
    public class GetPendingCardsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnOnlyPendingCardsInCreatedAtAscendingOrder()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var createdAt1 = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var createdAt2 = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc);
            var createdAt3 = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc);
            var createdAt4 = new DateTime(2024, 1, 4, 10, 0, 0, DateTimeKind.Utc);
            var createdAt5 = new DateTime(2024, 1, 5, 10, 0, 0, DateTimeKind.Utc);

            var pendingOldest = TestHelpers.CreateCard(Guid.NewGuid(), "user-1", "John Doe", "4242424242424242", CardEnum.Type.Credit, CardEnum.Brand.Visa, CardEnum.Status.Pending, 100m, 500m, DateTime.UtcNow.AddYears(2), createdAt1);
            var activeCard = TestHelpers.CreateCard(Guid.NewGuid(), "user-2", "Jane Roe", "5555555555554444", CardEnum.Type.Debit, CardEnum.Brand.Mastercard, CardEnum.Status.Active, 50m, null, DateTime.UtcNow.AddYears(3), createdAt2);
            var pendingMiddle = TestHelpers.CreateCard(Guid.NewGuid(), "user-3", "Alice Smith", "4000000000000002", CardEnum.Type.Credit, CardEnum.Brand.Visa, CardEnum.Status.Pending, 75m, 300m, DateTime.UtcNow.AddYears(4), createdAt3);
            var pendingNewest = TestHelpers.CreateCard(Guid.NewGuid(), "user-4", "Bob Brown", "6011000990139424", CardEnum.Type.Debit, CardEnum.Brand.Maestro, CardEnum.Status.Pending, 20m, null, DateTime.UtcNow.AddYears(5), createdAt4);
            var blockedCard = TestHelpers.CreateCard(Guid.NewGuid(), "user-5", "Carol White", "378282246310005", CardEnum.Type.Credit, CardEnum.Brand.AmericanExpress, CardEnum.Status.Blocked, 60m, 200m, DateTime.UtcNow.AddYears(6), createdAt5);

            context.Cards.AddRange(pendingOldest, activeCard, pendingMiddle, pendingNewest, blockedCard);
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetPendingCardsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetPendingCardsQuery();

            // Act
            List<CardDto> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, card => Assert.Equal(CardEnum.Status.Pending, context.Cards.Single(c => c.Id == card.Id).Status));
            Assert.Equal(new[] { pendingOldest.Id, pendingMiddle.Id, pendingNewest.Id }, result.Select(card => card.Id).ToArray());
            Assert.DoesNotContain(result, card => context.Cards.Single(c => c.Id == card.Id).Status != CardEnum.Status.Pending);
        }
    }
}
