using NobleBank.Domain.Events;
using static NobleBank.Domain.Common.Card;
using Type = NobleBank.Domain.Common.Card.Type;

namespace NobleBank.Domain.Entities
{
    public class Card : BaseEntity
    {
        public string CardNumber { get; private set; } = string.Empty;

        public string Last4Digits { get; private set; } = string.Empty;

        public string CardHolder { get; private set; } = string.Empty;

        public Type Type { get; private set; }

        public Brand Brand { get; private set; }

        public Status Status { get; private set; }

        public decimal Balance { get; private set; }

        public decimal? CreditLimit { get; private set; }

        public string Currency { get; private set; } = Common.Constants.Card.DefaultCurrency;

        public DateTime ExpiryDate { get; private set; }

        public string UserId { get; private set; } = string.Empty;

        public ApplicationUser User { get; private set; } = null!;

        private readonly List<Transaction> _transactions = [];   // -> ????

        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        // --- Audit fields ---
        public string? LastModifiedBy { get; private set; }

        public string? CreatedBy { get; private set; }

        private Card() { }

        public static Card Create(
         string cardHolder,
         string plainCardNumber,Common.Card.Type type,
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
                Type = type,
                Brand = brand,
                Status = Status.Pending,
                Balance = initialBalance,
                CreditLimit = type == Common.Card.Type.Credit ? creditLimit : null,
                ExpiryDate = DateTime.UtcNow.AddYears(4),
                UserId = userId,
                CreatedBy = createdBy
            };
        }

        public void Block(string reason, string performedBy)
        {
            Status = Status.Blocked;
            UpdatedAt = DateTime.UtcNow;
            LastModifiedBy = performedBy;

            AddDomainEvent(new CardBlockedEvent(Id, UserId, reason, performedBy, DateTime.UtcNow));
        }
    }
}