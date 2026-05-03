using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Features.Cards.Commands.RequestCard;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Tests.CardTests
{
    public class RequestCardCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenUserExists_ShouldCreateAndReturnCardDto()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            context.Users.Add(new ApplicationUser
            {
                Id = "user-1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new RequestCardCommandHandler(context, TestHelpers.CreateMapper());
            var command = new RequestCardCommand("user-1", CardEnum.Type.Credit, CardEnum.Brand.Visa, 250m);

            // Act
            CardDto result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("JOHN DOE", result.CardHolder);
            Assert.Equal("Visa", result.Brand);
            Assert.Equal("Credit", result.Type);
            Assert.Equal("Active", result.Status);
            Assert.Equal(0m, result.Balance);
            Assert.Equal(250m, result.CreditLimit);
            Assert.Equal("EUR", result.Currency);
            Assert.True(result.IsCredit);

            var persistedCard = await context.Cards.FirstOrDefaultAsync(c => c.Id == result.Id);
            Assert.NotNull(persistedCard);
            Assert.Equal("user-1", persistedCard!.UserId);
            Assert.Equal("user-1", persistedCard.CreatedBy);
            Assert.Equal("user-1", persistedCard.LastModifiedBy);
            Assert.Equal(CardEnum.Status.Active, persistedCard.Status);
        }

        [Fact]
        public async Task Handle_WhenUserIsMissing_ShouldThrowNotFoundException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var handler = new RequestCardCommandHandler(context, TestHelpers.CreateMapper());
            var command = new RequestCardCommand("missing-user", CardEnum.Type.Credit, CardEnum.Brand.Visa, 250m);

            // Act
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Contains("missing-user", ex.Message);
        }
    }
}
