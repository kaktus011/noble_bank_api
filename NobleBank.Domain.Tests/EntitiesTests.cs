using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
using NobleBank.Domain.Events;
using NobleBank.Domain.Interfaces;
using NobleBank.Domain.ValueObjects;

namespace NobleBank.Domain.Tests
{
    public class EntitiesTests
    {
        [Fact]
        public void BaseEntity_ShouldInitializeIdentityAndDates()
        {
            // Arrange
            var entity = CreatePendingTestCard();

            // Act

            // Assert
            Assert.NotEqual(Guid.Empty, entity.Id);
            Assert.True(entity.CreatedAt <= DateTime.Now);
            Assert.Equal(default, entity.UpdatedAt);
        }

        [Fact]
        public void BaseEntity_ClearDomainEvents_ShouldRemoveEvents()
        {
            // Arrange
            var card = CreatePendingTestCard();
            card.Block("Suspicious activity", "admin");

            // Act
            card.ClearDomainEvents();

            // Assert
            Assert.Empty(card.DomainEvents);
        }

        [Fact]
        public void Result_Success_ShouldSetSuccessState()
        {
            // Arrange
            var result = Result<int>.Success(42);

            // Act

            // Assert
            Assert.True(result.IsSucccess);
            Assert.False(result.IsFail);
            Assert.Equal(42, result.Value);
            Assert.Equal(string.Empty, result.Error);
        }

        [Fact]
        public void Result_Failure_ShouldSetFailureState()
        {
            // Arrange
            var result = Result<int>.Failure("error");

            // Act

            // Assert
            Assert.False(result.IsSucccess);
            Assert.True(result.IsFail);
            Assert.Equal(default(int), result.Value);
            Assert.Equal("error", result.Error);
        }

        [Fact]
        public void CardNumber_Create_WithValidLuhnNumber_ShouldCreateAndMask()
        {
            // Arrange
            var encryption = new FakeEncryptionService();

            // Act
            var cardNumber = CardNumber.Create("4242424242424242", encryption);

            // Assert
            Assert.Equal("4242", cardNumber.Last4);
            Assert.Equal("**** **** **** 4242", cardNumber.Masked);
        }

        [Fact]
        public void CardNumber_Create_WithInvalidNumber_ShouldThrowDomainException()
        {
            // Arrange
            var encryption = new FakeEncryptionService();

            // Act
            var ex = Assert.Throws<DomainException>(() => CardNumber.Create("1234567890123456", encryption));

            // Assert
            Assert.Equal("Invalid card number.", ex.Message);
        }

        [Fact]
        public void CardNumber_Equals_ShouldBeBasedOnEncryptedValue()
        {
            // Arrange
            var encryption = new FakeEncryptionService();

            var first = CardNumber.Create("4242424242424242", encryption);
            var second = CardNumber.Create("4242424242424242", encryption);
            var different = CardNumber.Create("4000000000000002", encryption);

            // Act

            // Assert
            Assert.Equal(first, second);
            Assert.NotEqual(first, different);
        }

        [Fact]
        public void ApplicationUser_FullName_ShouldCombineAndTrimNames()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var fullName = user.FullName;

            // Assert
            Assert.Equal("John Doe", fullName);
        }

        [Fact]
        public void Card_Create_ShouldInitializeCardState()
        {
            // Arrange
            // Act
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Credit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 100m,
                creditLimit: 500m);

            // Assert
            Assert.Equal("4242", card.Last4Digits);
            Assert.Equal("JOHN DOE", card.CardHolder);
            Assert.Equal(CardEnum.Type.Credit, card.CardType);
            Assert.Equal(CardEnum.Brand.Visa, card.Brand);
            Assert.Equal(CardEnum.Status.Pending, card.Status);
            Assert.Equal(100m, card.Balance);
            Assert.Equal(500m, card.CreditLimit);
            Assert.Equal("user-1", card.UserId);
            Assert.Equal("user-1", card.CreatedBy);
            Assert.Equal(Constants.Card.DefaultCurrency, card.Currency);
            Assert.False(card.IsExpired);
            Assert.Equal("**** **** **** 4242", card.MaskedNumber);
        }

        [Fact]
        public void Card_Activate_ShouldSetStatusAndAuditFields()
        {
            // Arrange
            var card = CreatePendingTestCard();

            // Act
            card.Activate("admin-1");

            // Assert
            Assert.Equal(CardEnum.Status.Active, card.Status);
            Assert.Equal("admin-1", card.LastModifiedBy);
            Assert.NotEqual(default, card.UpdatedAt);
        }

        [Fact]
        public void Card_Block_ShouldSetStatusAuditFieldsAndRaiseEvent()
        {
            // Arrange
            var card = CreateActiveTestCard();

            // Act
            card.Block("Suspicious activity", "admin-2");

            // Assert
            Assert.Equal(CardEnum.Status.Blocked, card.Status);
            Assert.Equal("admin-2", card.LastModifiedBy);
            Assert.Single(card.DomainEvents);

            var blockedEvent = Assert.IsType<CardBlockedEvent>(card.DomainEvents.First());
            Assert.Equal(card.Id, blockedEvent.CardId);
            Assert.Equal(card.UserId, blockedEvent.UserId);
            Assert.Equal("Suspicious activity", blockedEvent.Reason);
            Assert.Equal("admin-2", blockedEvent.PerformedBy);
        }

        [Fact]
        public void Card_Deposit_ShouldRejectInvalidAmount()
        {
            // Arrange
            var card = CreateActiveTestCard();

            // Act
            var result = card.Deposit(0, "admin");

            // Assert
            Assert.True(result.IsFail);
            Assert.Equal("Amount must be positive.", result.Error);
        }

        [Fact]
        public void Card_Deposit_OnInactiveCard_ShouldFail()
        {
            // Arrange
            var card = CreatePendingTestCard();

            // Act
            var result = card.Deposit(10m, "admin");

            // Assert
            Assert.True(result.IsFail);
            Assert.Equal("Card is not active.", result.Error);
        }

        [Fact]
        public void Card_Deposit_ShouldUpdateBalanceAndAuditFields()
        {
            // Arrange
            var card = CreateActiveTestCard(balance: 100m);

            // Act
            var result = card.Deposit(25m, "admin");

            // Assert
            Assert.True(result.IsSucccess);
            Assert.Equal(125m, result.Value);
            Assert.Equal(125m, card.Balance);
            Assert.Equal("admin", card.LastModifiedBy);
        }

        [Fact]
        public void Card_Withdraw_ShouldRejectInvalidAmount()
        {
            // Arrange
            var card = CreateActiveTestCard(balance: 100m);

            // Act
            var result = card.Withdraw(0, "admin");

            // Assert
            Assert.True(result.IsFail);
            Assert.Equal("Amount must be positive.", result.Error);
        }

        [Fact]
        public void Card_Withdraw_OnInactiveCard_ShouldFail()
        {
            // Arrange
            var card = CreatePendingTestCard();

            // Act
            var result = card.Withdraw(10m, "admin");

            // Assert
            Assert.True(result.IsFail);
            Assert.Equal("Card is not active.", result.Error);
        }

        [Fact]
        public void Card_Withdraw_OnExpiredCard_ShouldFail()
        {
            // Arrange
            var card = CreateActiveTestCard(balance: 100m);
            SetPrivateProperty(card, nameof(Card.ExpiryDate), DateTime.UtcNow.AddDays(-1));

            // Act
            var result = card.Withdraw(10m, "admin");

            // Assert
            Assert.True(result.IsFail);
            Assert.Equal("Card is expired.", result.Error);
        }

        [Fact]
        public void Card_Withdraw_WithInsufficientFunds_ShouldFail()
        {
            // Arrange
            var card = CreateActiveDebitTestCard(balance: 100m);

            // Act
            var result = card.Withdraw(101m, "admin");

            // Assert
            Assert.True(result.IsFail);
            Assert.Equal("Insufficient funds.", result.Error);
        }

        [Fact]
        public void Card_Withdraw_ShouldUpdateBalanceAndAuditFields()
        {
            // Arrange
            var card = CreateActiveTestCard(balance: 100m);

            // Act
            var result = card.Withdraw(40m, "admin");

            // Assert
            Assert.True(result.IsSucccess);
            Assert.Equal(60m, result.Value);
            Assert.Equal(60m, card.Balance);
            Assert.Equal("admin", card.LastModifiedBy);
        }

        [Fact]
        public void Loan_Create_ShouldInitializeState()
        {
            // Arrange
            // Act
            var loan = Loan.Create("Car loan", 10000m, 5.5m, "user-1", "user-1");

            // Assert
            Assert.Equal("Car loan", loan.Name);
            Assert.Equal(10000m, loan.Amount);
            Assert.Equal(10000m, loan.RemainingAmount);
            Assert.Equal(5.5m, loan.InterestRate);
            Assert.Equal(LoansEnum.Status.Active, loan.Status);
            Assert.Equal("user-1", loan.UserId);
            Assert.Equal("user-1", loan.CreatedBy);
        }

        [Fact]
        public void Loan_ApplyPayment_WithNegativeAmount_ShouldThrow()
        {
            // Arrange
            var loan = Loan.Create("Car loan", 1000m, 5m, "user-1", "user-1");

            // Act
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => loan.ApplyPayment(-1m));

            // Assert
            Assert.Equal("paymentAmount", ex.ParamName);
        }

        [Fact]
        public void Loan_ApplyPayment_AboveRemainingAmount_ShouldThrow()
        {
            // Arrange
            var loan = Loan.Create("Car loan", 1000m, 5m, "user-1", "user-1");

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => loan.ApplyPayment(1001m));

            // Assert
            Assert.Equal("Payment amount cannot exceed the remaining loan amount.", ex.Message);
        }

        [Fact]
        public void Loan_ApplyPayment_ShouldReduceRemainingAmount()
        {
            // Arrange
            var loan = Loan.Create("Car loan", 1000m, 5m, "user-1", "user-1");

            // Act
            loan.ApplyPayment(250m);

            // Assert
            Assert.Equal(750m, loan.RemainingAmount);
        }

        [Fact]
        public void Transaction_Create_ShouldInitializeFields()
        {
            // Arrange
            var cardId = Guid.NewGuid();

            // Act
            var transaction = Transaction.Create(
                cardId: cardId,
                amount: 150m,
                currency: "EUR",
                type: TransactionsEnum.Type.Income,
                description: "Salary",
                createdBy: "user-1",
                receiverAccount: "receiver",
                senderAccount: "sender");

            // Assert
            Assert.Equal(cardId, transaction.CardId);
            Assert.Equal(150m, transaction.Amount);
            Assert.Equal("EUR", transaction.Currency);
            Assert.Equal(TransactionsEnum.Type.Income, transaction.Type);
            Assert.Equal("Salary", transaction.Description);
            Assert.Equal("user-1", transaction.CreatedBy);
            Assert.Equal("receiver", transaction.ReceiverAccount);
            Assert.Equal("sender", transaction.SenderAccount);
            Assert.NotEqual(default, transaction.OccurredAt);
        }

        private static Card CreateActiveTestCard(decimal balance = 0m)
        {
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Credit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: balance,
                creditLimit: 500m);

            card.Activate("user-1");
            return card;
        }

        private static Card CreateActiveDebitTestCard(decimal balance = 0m)
        {
            var card = Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Debit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: balance,
                creditLimit: null);

            card.Activate("user-1");
            return card;
        }

        private static Card CreatePendingTestCard()
        {
            return Card.Create(
                cardHolder: "John Doe",
                plainCardNumber: "4242424242424242",
                type: CardEnum.Type.Credit,
                brand: CardEnum.Brand.Visa,
                userId: "user-1",
                createdBy: "user-1",
                initialBalance: 0m,
                creditLimit: 500m);
        }

        private static void SetPrivateProperty<T>(T instance, string propertyName, object value)
        {
            var property = typeof(T).GetProperty(propertyName);
            Assert.NotNull(property);
            property!.SetValue(instance, value);
        }

        private sealed class FakeEncryptionService : IEncryptionService
        {
            public string Encrypt(string plainText) => $"enc:{plainText}";
            public string Decrypt(string cipherText) => cipherText.StartsWith("enc:") ? cipherText[4..] : cipherText;
        }
    }
}