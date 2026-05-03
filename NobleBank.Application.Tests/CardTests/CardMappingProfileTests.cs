using NobleBank.Application.Features.Cards;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.CardTests
{
    public class CardMappingProfileTests
    {
        [Fact]
        public void Map_CardToCardDto_ShouldMapExpectedValues()
        {
            // Arrange
            var mapper = TestHelpers.CreateMapper();
            var card = TestHelpers.CreateCard(
                id: Guid.NewGuid(),
                userId: "user-1",
                cardHolder: "John Doe",
                cardNumber: "4242424242424242",
                type: CardEnum.Type.Credit,
                brand: CardEnum.Brand.Visa,
                status: CardEnum.Status.Active,
                balance: 100m,
                creditLimit: 500m,
                expiryDate: DateTime.UtcNow.AddYears(2),
                createdAt: DateTime.UtcNow.AddDays(-1));

            // Act
            var dto = mapper.Map<CardDto>(card);

            // Assert
            Assert.Equal(card.Id, dto.Id);
            Assert.Equal("Visa", dto.Brand);
            Assert.Equal("4242", dto.Last4Digits);
            Assert.Equal("JOHN DOE", dto.CardHolder);
            Assert.Equal("Credit", dto.Type);
            Assert.Equal("Active", dto.Status);
            Assert.Equal(100m, dto.Balance);
            Assert.Equal(500m, dto.CreditLimit);
            Assert.Equal(Constants.Card.DefaultCurrency, dto.Currency);
            Assert.Equal(card.ExpiryDate, dto.ExpiryDate);
            Assert.True(dto.IsCredit);
            Assert.False(dto.IsExpired);
        }
    }
}
