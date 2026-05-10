using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Application.Features.Transactions.Queries.GetTransactionById;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.TransactionTests
{
    public class GetTransactionByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenTransactionExistsForUser_ShouldReturnTransactionDto()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var transactionId = Guid.NewGuid();
            var transaction = TestHelpers.CreateTransaction(
                transactionId,
                card.Id,
                100m,
                "Test transaction",
                TransactionsEnum.Type.Expense,
                DateTime.UtcNow);

            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var handler = new GetTransactionByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetTransactionByIdQuery(transactionId, userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result!.Id);
            Assert.Equal(100m, result.Amount);
            Assert.Equal("Test transaction", result.Description);
            Assert.Equal("Expense", result.Type);
            Assert.Equal(card.Id, result.CardId);
        }

        [Fact]
        public async Task Handle_WhenTransactionExistsButNotForUser_ShouldReturnNull()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId1 = "user-1";
            var userId2 = "user-2";
            var user1 = TestHelpers.CreateUser(userId1);
            var user2 = TestHelpers.CreateUser(userId2);
            var card = TestHelpers.CreateAndActivateCard(context, userId2);

            context.Users.Add(user1);
            context.Users.Add(user2);
            await context.SaveChangesAsync();

            var transactionId = Guid.NewGuid();
            var transaction = TestHelpers.CreateTransaction(
                transactionId,
                card.Id,
                100m,
                "Test transaction",
                TransactionsEnum.Type.Expense,
                DateTime.UtcNow);

            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var handler = new GetTransactionByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetTransactionByIdQuery(transactionId, userId1);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_WhenTransactionDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new GetTransactionByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetTransactionByIdQuery(Guid.NewGuid(), userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ShouldMapTransactionPropertiesToDto()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var transactionId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var transaction = TestHelpers.CreateTransaction(
                transactionId,
                card.Id,
                250.75m,
                "Restaurant dinner",
                TransactionsEnum.Type.Expense,
                now,
                "user-1");

            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var handler = new GetTransactionByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetTransactionByIdQuery(transactionId, userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result!.Id);
            Assert.Equal(250.75m, result.Amount);
            Assert.Equal("Restaurant dinner", result.Description);
            Assert.Equal("Expense", result.Type);
            Assert.Equal(card.Id, result.CardId);
            Assert.Equal(now, result.OccurredAt);
            Assert.NotEmpty(result.CardLast4);
        }

        [Fact]
        public async Task Handle_WithIncomeTransaction_ShouldReturnCorrectType()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var transactionId = Guid.NewGuid();
            var transaction = TestHelpers.CreateTransaction(
                transactionId,
                card.Id,
                1000m,
                "Salary",
                TransactionsEnum.Type.Income,
                DateTime.UtcNow);

            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var handler = new GetTransactionByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetTransactionByIdQuery(transactionId, userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Income", result!.Type);
        }
    }
}
