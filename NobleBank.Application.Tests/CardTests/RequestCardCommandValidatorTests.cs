using FluentValidation;
using NobleBank.Application.Features.Cards.Commands.RequestCard;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.CardTests
{
    public class RequestCardCommandValidatorTests
    {
        [Fact]
        public void Validate_WithValidCreditCardRequest_ShouldPass()
        {
            // Arrange
            var validator = new RequestCardCommandValidator();
            var command = new RequestCardCommand("user-1", CardEnum.Type.Credit, CardEnum.Brand.Visa, 500m);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WhenCreditLimitMissingForCreditCard_ShouldFail()
        {
            // Arrange
            var validator = new RequestCardCommandValidator();
            var command = new RequestCardCommand("user-1", CardEnum.Type.Credit, CardEnum.Brand.Visa, null);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(RequestCardCommand.CreditLimit));
        }

        [Fact]
        public void Validate_WithDebitCardAndNullCreditLimit_ShouldPass()
        {
            // Arrange
            var validator = new RequestCardCommandValidator();
            var command = new RequestCardCommand("user-1", CardEnum.Type.Debit, CardEnum.Brand.Visa, null);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
