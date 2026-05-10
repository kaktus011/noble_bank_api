using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Features.Transactions.Commands.CreateTransaction;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Tests.TransactionTests
{
    public class CreateTransactionCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WithValidExpenseCommand_ShouldCreateTransactionAndWithdrawFromCard()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId, CardEnum.Type.Debit, 1000m);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: card.Id,
                Amount: 100m,
                Description: "Coffee purchase",
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(100m, result.Amount);
            Assert.Equal("Coffee purchase", result.Description);
            Assert.Equal("Expense", result.Type);
            Assert.Equal(card.Id, result.CardId);
            Assert.NotEqual(default, result.OccurredAt);

            var updatedCard = await context.Cards.FirstOrDefaultAsync(c => c.Id == card.Id);
            Assert.NotNull(updatedCard);
            Assert.Equal(900m, updatedCard!.Balance);

            var persistedTransaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == result.Id);
            Assert.NotNull(persistedTransaction);
            Assert.Equal(userId, persistedTransaction!.PerformedBy);
        }

        [Fact]
        public async Task Handle_WithValidIncomeCommand_ShouldCreateTransactionAndDepositToCard()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId, CardEnum.Type.Debit, 1000m);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: card.Id,
                Amount: 500m,
                Description: "Salary deposit",
                Type: TransactionsEnum.Type.Income);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(500m, result.Amount);
            Assert.Equal("Salary deposit", result.Description);
            Assert.Equal("Income", result.Type);

            var updatedCard = await context.Cards.FirstOrDefaultAsync(c => c.Id == card.Id);
            Assert.NotNull(updatedCard);
            Assert.Equal(1500m, updatedCard!.Balance);
        }

        [Fact]
        public async Task Handle_WithInsufficientFunds_ShouldThrowDomainException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId, CardEnum.Type.Debit, 100m);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: card.Id,
                Amount: 500m,
                Description: "Expense exceeding balance",
                Type: TransactionsEnum.Type.Expense);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Insufficient funds.", ex.Message);
        }

        [Fact]
        public async Task Handle_WithInactiveCard_ShouldThrowDomainException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = Card.Create(
                cardHolder: "Test User",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Debit,
                brand: CardEnum.Brand.Visa,
                userId: userId,
                createdBy: userId,
                initialBalance: 1000m,
                creditLimit: null);

            context.Users.Add(user);
            context.Cards.Add(card);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: card.Id,
                Amount: 100m,
                Description: "Test",
                Type: TransactionsEnum.Type.Expense);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Card is not active.", ex.Message);
        }

        [Fact]
        public async Task Handle_WithExpiredCard_ShouldThrowDomainException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId, CardEnum.Type.Debit, 1000m);

            // Set expiry date in the past
            var expiryDateProperty = typeof(Card).GetProperty(nameof(Card.ExpiryDate));
            expiryDateProperty?.SetValue(card, DateTime.UtcNow.AddMonths(-1));

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: card.Id,
                Amount: 100m,
                Description: "Test",
                Type: TransactionsEnum.Type.Expense);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Card is expired.", ex.Message);
        }

        [Fact]
        public async Task Handle_WithNonExistentCard_ShouldThrowNotFoundException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: Guid.NewGuid(),
                Amount: 100m,
                Description: "Test",
                Type: TransactionsEnum.Type.Expense);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Card not found or does not belong to user", ex.Message);
        }

        [Fact]
        public async Task Handle_WithCardBelongingToAnotherUser_ShouldThrowNotFoundException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId1 = "user-1";
            var userId2 = "user-2";
            var user1 = TestHelpers.CreateUser(userId1);
            var user2 = TestHelpers.CreateUser(userId2);
            var cardOwnedByUser2 = TestHelpers.CreateAndActivateCard(context, userId2, CardEnum.Type.Debit, 1000m);

            context.Users.Add(user1);
            context.Users.Add(user2);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId1,
                CardId: cardOwnedByUser2.Id,
                Amount: 100m,
                Description: "Test",
                Type: TransactionsEnum.Type.Expense);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Card not found or does not belong to user", ex.Message);
        }

        [Fact]
        public async Task Handle_WithEmptyUserId_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: string.Empty,
                CardId: Guid.NewGuid(),
                Amount: 100m,
                Description: "Test",
                Type: TransactionsEnum.Type.Expense);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("User ID is required", ex.Message);
        }

        [Fact]
        public async Task Handle_WithCreditCardAndWithdrawWithinLimit_ShouldSucceed()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId, CardEnum.Type.Credit, 500m, 1000m);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: card.Id,
                Amount: 800m,
                Description: "Credit card purchase",
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(800m, result.Amount);

            var updatedCard = await context.Cards.FirstOrDefaultAsync(c => c.Id == card.Id);
            Assert.NotNull(updatedCard);
            Assert.Equal(-300m, updatedCard!.Balance); // 500 - 800 = -300
        }

        [Fact]
        public async Task Handle_WithCreditCardExceedingLimit_ShouldThrowDomainException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId, CardEnum.Type.Credit, 500m, 1000m);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: card.Id,
                Amount: 1600m,
                Description: "Exceeds credit limit",
                Type: TransactionsEnum.Type.Expense);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Insufficient funds.", ex.Message);
        }

        [Fact]
        public async Task Handle_WithTransferType_ShouldThrowDomainException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId, CardEnum.Type.Debit, 1000m);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreateTransactionCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: card.Id,
                Amount: 100m,
                Description: "Transfer",
                Type: TransactionsEnum.Type.Transfer);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Transfer type not yet implemented", ex.Message);
        }
    }
}
