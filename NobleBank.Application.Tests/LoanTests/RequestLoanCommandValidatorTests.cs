using NobleBank.Application.Features.Loans.Commands.RequestLoan;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.LoanTests
{
    public class RequestLoanCommandValidatorTests
    {
        [Fact]
        public void Validate_WithValidRequest_ShouldPass()
        {
            // Arrange
            var validator = new RequestLoanCommandValidator();
            var command = new RequestLoanCommand("user-1", 5000m, 60, LoansEnum.Type.Personal);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WithInvalidAmount_ShouldFail()
        {
            // Arrange
            var validator = new RequestLoanCommandValidator();
            var command = new RequestLoanCommand("user-1", 0m, 60, LoansEnum.Type.Personal);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(RequestLoanCommand.Amount));
        }

        [Fact]
        public void Validate_WithInvalidTerm_ShouldFail()
        {
            // Arrange
            var validator = new RequestLoanCommandValidator();
            var command = new RequestLoanCommand("user-1", 5000m, 0, LoansEnum.Type.Personal);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(RequestLoanCommand.TermMonths));
        }
    }
}
