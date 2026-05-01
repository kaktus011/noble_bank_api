using NobleBank.Domain.Events;
using NobleBank.Domain.Common;
using static NobleBank.Domain.Common.CardEnum;
using Type = NobleBank.Domain.Common.CardEnum.Type;

namespace NobleBank.Domain.Entities
{
    public class Card : BaseEntity
    {
        public string CardNumber { get; private set; } = string.Empty;

        public string Last4Digits { get; private set; } = string.Empty;

        public string CardHolder { get; private set; } = string.Empty;

        public Type CardType { get; private set; }

        public Brand Brand { get; private set; }

        public Status Status { get; private set; }

        public decimal Balance { get; private set; }

        public decimal? CreditLimit { get; private set; }

        public string Currency { get; private set; } = Constants.Card.DefaultCurrency;

        public DateTime ExpiryDate { get; private set; }

        public string UserId { get; private set; } = string.Empty;

        public ApplicationUser User { get; private set; } = null!;

        private readonly List<Transaction> _transactions = [];

        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        public string MaskedNumber => $"**** **** **** {Last4Digits}";

        public bool IsExpired => ExpiryDate < DateTime.UtcNow;

        // --- Audit fields ---
        public string? LastModifiedBy { get; private set; }

        public string? CreatedBy { get; private set; }

        private Card() { }

        public static Card Create(
         string cardHolder,
         string plainCardNumber,
         Type type,
         Brand brand,
         string userId,
         string createdBy,
         decimal initialBalance = 0,
         decimal? creditLimit = null)
        {
            return new Card
            {
                CardNumber = plainCardNumber,
                Last4Digits = plainCardNumber[^4..],
                CardHolder = cardHolder.ToUpper().Trim(),
                CardType = type,
                Brand = brand,
                Status = Status.Pending,
                Balance = initialBalance,
                CreditLimit = type == Type.Credit ? creditLimit : null,
                ExpiryDate = DateTime.UtcNow.AddYears(4),
                UserId = userId,
                CreatedBy = createdBy
            };
        }

        public void Activate(string performedBy)
        {
            Status = Status.Active;
            UpdatedAt = DateTime.UtcNow;
            LastModifiedBy = performedBy;
        }

        public void Block(string reason, string performedBy)
        {
            Status = Status.Blocked;
            UpdatedAt = DateTime.UtcNow;
            LastModifiedBy = performedBy;

            AddDomainEvent(new CardBlockedEvent(Id, UserId, reason, performedBy, DateTime.UtcNow));
        }

        public Result<decimal> Deposit(decimal amount, string performedBy)
        {
            if (amount <= 0)
            {
                return Result<decimal>.Failure("Amount must be positive.");
            }

            if (Status != Status.Active)
            {
                return Result<decimal>.Failure("Card is not active.");
            }

            Balance += amount;
            UpdatedAt = DateTime.UtcNow;
            LastModifiedBy = performedBy;

            return Result<decimal>.Success(Balance);
        }

        public Result<decimal> Withdraw(decimal amount, string performedBy)
        {
            if (amount <= 0)
            {
                return Result<decimal>.Failure("Amount must be positive.");
            }

            if (Status != Status.Active)
            {
                return Result<decimal>.Failure("Card is not active.");
            }

            if (IsExpired)
            {
                return Result<decimal>.Failure("Card is expired.");
            }

            var available = CardType == Type.Credit
                ? Balance + (CreditLimit ?? 0)
                : Balance;

            if (amount > available)
            {
                return Result<decimal>.Failure("Insufficient funds.");
            }

            Balance -= amount;
            UpdatedAt = DateTime.UtcNow;
            LastModifiedBy = performedBy;

            return Result<decimal>.Success(Balance);
        }
    }
}