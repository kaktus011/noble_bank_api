using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.CardTests
{
    public class GetAllCardsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnOnlyCurrentUsersCardsInDescendingOrder()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            context.Cards.AddRange(
                TestHelpers.CreateCard(Guid.NewGuid(), "user-1", "John Doe", "4242424242424242", CardEnum.Type.Credit, CardEnum.Brand.Visa, CardEnum.Status.Active, 100m, 500m, DateTime.UtcNow.AddYears(2), DateTime.UtcNow.AddDays(-2)),
                TestHelpers.CreateCard(Guid.NewGuid(), "user-1", "John Doe", "5555555555554444", CardEnum.Type.Debit, CardEnum.Brand.Mastercard, CardEnum.Status.Active, 50m, null, DateTime.UtcNow.AddYears(3), DateTime.UtcNow.AddDays(-1)),
                TestHelpers.CreateCard(Guid.NewGuid(), "user-2", "Jane Roe", "4000000000000002", CardEnum.Type.Credit, CardEnum.Brand.Visa, CardEnum.Status.Active, 75m, 300m, DateTime.UtcNow.AddYears(4), DateTime.UtcNow.AddDays(-3)));

            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetAllCardsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllCardsQuery("user-1");

            // Act
            List<CardDto> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, card => Assert.Equal("user-1", context.Cards.Single(c => c.Id == card.Id).UserId));
            Assert.Contains(result, card => card.Last4Digits == "4242");
            Assert.Contains(result, card => card.Last4Digits == "4444");
            Assert.All(result, card => Assert.Equal("JOHN DOE", card.CardHolder));
            Assert.DoesNotContain(result, card => card.Last4Digits == "0002");
        }
    }
}
