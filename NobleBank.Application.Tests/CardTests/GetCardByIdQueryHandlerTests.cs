using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Application.Features.Cards.Queries.GetCardById;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.CardTests
{
    public class GetCardByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenCardExistsForUser_ShouldReturnCardDto()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var card = TestHelpers.CreateCard(Guid.NewGuid(), "user-1", "John Doe", "4242424242424242", CardEnum.Type.Credit, CardEnum.Brand.Visa, CardEnum.Status.Active, 100m, 500m, DateTime.UtcNow.AddYears(2), DateTime.UtcNow.AddDays(-1));
            context.Cards.Add(card);
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetCardByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetCardByIdQuery(card.Id, "user-1");

            // Act
            CardDto? result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(card.Id, result!.Id);
            Assert.Equal("JOHN DOE", result.CardHolder);
            Assert.Equal("Visa", result.Brand);
            Assert.Equal("Active", result.Status);
        }

        [Fact]
        public async Task Handle_WhenCardDoesNotMatchUser_ShouldReturnNull()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var card = TestHelpers.CreateCard(Guid.NewGuid(), "user-1", "John Doe", "4242424242424242", CardEnum.Type.Credit, CardEnum.Brand.Visa, CardEnum.Status.Active, 100m, 500m, DateTime.UtcNow.AddYears(2), DateTime.UtcNow.AddDays(-1));
            context.Cards.Add(card);
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetCardByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetCardByIdQuery(card.Id, "user-2");

            // Act
            CardDto? result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
