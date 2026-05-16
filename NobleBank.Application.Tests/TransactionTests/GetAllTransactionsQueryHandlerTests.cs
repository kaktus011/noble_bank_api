using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.TransactionTests
{
    public class GetAllTransactionsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithValidUserAndCard_ShouldReturnAllUserTransactions()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var transaction1 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card,
                100m,
                "Transaction 1",
                TransactionsEnum.Type.Expense,
                DateTime.UtcNow.AddHours(-2));

            var transaction2 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card,
                200m,
                "Transaction 2",
                TransactionsEnum.Type.Income,
                DateTime.UtcNow.AddHours(-1));

            context.Transactions.Add(transaction1);
            context.Transactions.Add(transaction2);
            await context.SaveChangesAsync();

            var handler = new GetAllTransactionsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllTransactionsQuery(userId, null, 50);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, t => Assert.True(t.Amount > 0));
            Assert.All(result, t => Assert.NotEqual(default, t.OccurredAt));
        }

        [Fact]
        public async Task Handle_WithCardIdFilter_ShouldReturnOnlyTransactionsForThatCard()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card1 = TestHelpers.CreateAndActivateCard(context, userId);
            var card2 = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var transaction1 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card1,
                100m,
                "Transaction 1",
                TransactionsEnum.Type.Expense,
                DateTime.UtcNow);

            var transaction2 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card2,
                200m,
                "Transaction 2",
                TransactionsEnum.Type.Income,
                DateTime.UtcNow);

            context.Transactions.Add(transaction1);
            context.Transactions.Add(transaction2);
            await context.SaveChangesAsync();

            var handler = new GetAllTransactionsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllTransactionsQuery(userId, card1.Id, 50);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(card1.Id, result[0].CardId);
            Assert.Equal(100m, result[0].Amount);
        }

        [Fact]
        public async Task Handle_WithLimitFilter_ShouldReturnLimitedResults()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            for (int i = 0; i < 10; i++)
            {
                var transaction = TestHelpers.CreateTransaction(
                    Guid.NewGuid(),
                    card,
                    100m * (i + 1),
                    $"Transaction {i + 1}",
                    TransactionsEnum.Type.Expense,
                    DateTime.UtcNow.AddHours(-i));

                context.Transactions.Add(transaction);
            }
            await context.SaveChangesAsync();

            var handler = new GetAllTransactionsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllTransactionsQuery(userId, null, 5);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task Handle_WithoutCardIdFilter_ShouldReturnAllUserCardsTransactions()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card1 = TestHelpers.CreateAndActivateCard(context, userId);
            var card2 = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var transaction1 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card1,
                100m,
                "Transaction 1",
                TransactionsEnum.Type.Expense,
                DateTime.UtcNow);

            var transaction2 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card2,
                200m,
                "Transaction 2",
                TransactionsEnum.Type.Income,
                DateTime.UtcNow);

            context.Transactions.Add(transaction1);
            context.Transactions.Add(transaction2);
            await context.SaveChangesAsync();

            var handler = new GetAllTransactionsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllTransactionsQuery(userId, null, 50);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Handle_WithTransactionFromAnotherUser_ShouldNotReturnIt()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId1 = "user-1";
            var userId2 = "user-2";
            var user1 = TestHelpers.CreateUser(userId1);
            var user2 = TestHelpers.CreateUser(userId2);
            var card1 = TestHelpers.CreateAndActivateCard(context, userId1);
            var card2 = TestHelpers.CreateAndActivateCard(context, userId2);

            context.Users.Add(user1);
            context.Users.Add(user2);
            await context.SaveChangesAsync();

            var transaction1 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card1,
                100m,
                "Transaction 1",
                TransactionsEnum.Type.Expense,
                DateTime.UtcNow);

            var transaction2 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card2,
                200m,
                "Transaction 2",
                TransactionsEnum.Type.Income,
                DateTime.UtcNow);

            context.Transactions.Add(transaction1);
            context.Transactions.Add(transaction2);
            await context.SaveChangesAsync();

            var handler = new GetAllTransactionsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllTransactionsQuery(userId1, null, 50);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(card1.Id, result[0].CardId);
        }

        [Fact]
        public async Task Handle_WithNoTransactions_ShouldReturnEmptyList()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new GetAllTransactionsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllTransactionsQuery(userId, null, 50);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnTransactionsSortedByOccurredAtDescending()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            var card = TestHelpers.CreateAndActivateCard(context, userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var transaction1 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card,
                100m,
                "Transaction 1",
                TransactionsEnum.Type.Expense,
                now.AddHours(-2));

            var transaction2 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card,
                200m,
                "Transaction 2",
                TransactionsEnum.Type.Income,
                now);

            var transaction3 = TestHelpers.CreateTransaction(
                Guid.NewGuid(),
                card,
                300m,
                "Transaction 3",
                TransactionsEnum.Type.Expense,
                now.AddHours(-1));

            context.Transactions.Add(transaction1);
            context.Transactions.Add(transaction2);
            context.Transactions.Add(transaction3);
            await context.SaveChangesAsync();

            var handler = new GetAllTransactionsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllTransactionsQuery(userId, null, 50);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(200m, result[0].Amount);
            Assert.Equal(300m, result[1].Amount);
            Assert.Equal(100m, result[2].Amount);
        }
    }
}
