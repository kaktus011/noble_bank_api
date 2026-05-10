using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Tests.TransactionTests
{
    public class TransactionMappingProfileTests
    {
        [Fact]
        public void Map_TransactionToTransactionDto_ShouldMapExpectedValues()
        {
            // Arrange
            var mapper = TestHelpers.CreateMapper();
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Debit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 1000m,
                creditLimit: null);

            var transaction = Transaction.Create(
                amount: 150.50m,
                description: "Coffee",
                type: TransactionsEnum.Type.Expense,
                cardId: card.Id,
                performedBy: "user-1");

            // Act
            var dto = mapper.Map<TransactionDto>(transaction);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(transaction.Id, dto.Id);
            Assert.Equal(150.50m, dto.Amount);
            Assert.Equal("Coffee", dto.Description);
            Assert.Equal("Expense", dto.Type);
            Assert.Equal(card.Id, dto.CardId);
            Assert.Equal(transaction.OccurredAt, dto.OccurredAt);
        }

        [Fact]
        public void Map_IncomeTransaction_ShouldShowIncomeType()
        {
            // Arrange
            var mapper = TestHelpers.CreateMapper();
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Debit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 1000m,
                creditLimit: null);

            var transaction = Transaction.Create(
                amount: 5000m,
                description: "Monthly salary",
                type: TransactionsEnum.Type.Income,
                cardId: card.Id,
                performedBy: "user-1");

            // Act
            var dto = mapper.Map<TransactionDto>(transaction);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal("Income", dto.Type);
            Assert.Equal(5000m, dto.Amount);
        }

        [Fact]
        public void Map_ExpenseTransaction_ShouldShowExpenseType()
        {
            // Arrange
            var mapper = TestHelpers.CreateMapper();
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Debit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 1000m,
                creditLimit: null);

            var transaction = Transaction.Create(
                amount: 45.99m,
                description: "Dinner",
                type: TransactionsEnum.Type.Expense,
                cardId: card.Id,
                performedBy: "user-1");

            // Act
            var dto = mapper.Map<TransactionDto>(transaction);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal("Expense", dto.Type);
            Assert.Equal(45.99m, dto.Amount);
        }

        [Fact]
        public void Map_TransactionWithCardLast4_ShouldMapCardLast4FromCard()
        {
            // Arrange
            var mapper = TestHelpers.CreateMapper();
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Debit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 1000m,
                creditLimit: null);

            var transaction = Transaction.Create(
                amount: 100m,
                description: "Test",
                type: TransactionsEnum.Type.Expense,
                cardId: card.Id,
                performedBy: "user-1");

            // Manually set the Card navigation property
            var cardProperty = typeof(Transaction).GetProperty(nameof(Transaction.Card));
            cardProperty?.SetValue(transaction, card);

            // Act
            var dto = mapper.Map<TransactionDto>(transaction);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal("4242", dto.CardLast4);
        }

        [Fact]
        public void Map_MultipleTransactions_ShouldMapAllCorrectly()
        {
            // Arrange
            var mapper = TestHelpers.CreateMapper();
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Debit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 1000m,
                creditLimit: null);

            var transaction1 = Transaction.Create(
                amount: 100m,
                description: "Transaction 1",
                type: TransactionsEnum.Type.Expense,
                cardId: card.Id,
                performedBy: "user-1");

            var transaction2 = Transaction.Create(
                amount: 500m,
                description: "Transaction 2",
                type: TransactionsEnum.Type.Income,
                cardId: card.Id,
                performedBy: "user-1");

            var transactions = new List<Transaction> { transaction1, transaction2 };

            // Act
            var dtos = mapper.Map<List<TransactionDto>>(transactions);

            // Assert
            Assert.NotNull(dtos);
            Assert.Equal(2, dtos.Count);
            Assert.Equal(100m, dtos[0].Amount);
            Assert.Equal(500m, dtos[1].Amount);
            Assert.Equal("Expense", dtos[0].Type);
            Assert.Equal("Income", dtos[1].Type);
        }

        [Fact]
        public void Map_TransactionWithLargeAmount_ShouldMapCorrectly()
        {
            // Arrange
            var mapper = TestHelpers.CreateMapper();
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Debit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 10000m,
                creditLimit: null);

            var transaction = Transaction.Create(
                amount: 9999.99m,
                description: "Large transaction",
                type: TransactionsEnum.Type.Expense,
                cardId: card.Id,
                performedBy: "user-1");

            // Act
            var dto = mapper.Map<TransactionDto>(transaction);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(9999.99m, dto.Amount);
        }
    }
}
