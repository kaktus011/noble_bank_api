using FluentValidation.TestHelper;
using NobleBank.Application.Features.Transactions.Commands.CreateTransaction;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.TransactionTests
{
    public class CreateTransactionCommandValidatorTests
    {
        private readonly CreateTransactionCommandValidator _validator = new();

        [Fact]
        public void Validate_WithValidCommand_ShouldPass()
        {
            // Arrange
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.NewGuid(),
                Amount: 100m,
                Description: "Test transaction",
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyCardId_ShouldFail()
        {
            // Arrange
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.Empty,
                Amount: 100m,
                Description: "Test transaction",
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CardId)
                .WithErrorMessage("Card ID is required.");
        }

        [Fact]
        public void Validate_WithNegativeAmount_ShouldFail()
        {
            // Arrange
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.NewGuid(),
                Amount: -100m,
                Description: "Test transaction",
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Transaction amount must be greater than 0.");
        }

        [Fact]
        public void Validate_WithZeroAmount_ShouldFail()
        {
            // Arrange
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.NewGuid(),
                Amount: 0m,
                Description: "Test transaction",
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Validate_WithEmptyDescription_ShouldFail()
        {
            // Arrange
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.NewGuid(),
                Amount: 100m,
                Description: string.Empty,
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("Transaction description is required.");
        }

        [Fact]
        public void Validate_WithDescriptionExceedingMaxLength_ShouldFail()
        {
            // Arrange
            var longDescription = new string('a', 251);
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.NewGuid(),
                Amount: 100m,
                Description: longDescription,
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("Transaction description cannot exceed 250 characters.");
        }

        [Fact]
        public void Validate_WithInvalidTransactionType_ShouldFail()
        {
            // Arrange
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.NewGuid(),
                Amount: 100m,
                Description: "Test",
                Type: (TransactionsEnum.Type)999);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Type);
        }

        [Fact]
        public void Validate_WithIncomeType_ShouldPass()
        {
            // Arrange
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.NewGuid(),
                Amount: 500m,
                Description: "Salary deposit",
                Type: TransactionsEnum.Type.Income);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithMaxLengthDescription_ShouldPass()
        {
            // Arrange
            var description = new string('a', 250);
            var command = new CreateTransactionCommand(
                UserId: "user-1",
                CardId: Guid.NewGuid(),
                Amount: 100m,
                Description: description,
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
