using NobleBank.Domain.Common;
using NobleBank.Domain.Events;
using NobleBank.Domain.Interfaces;
using static NobleBank.Domain.Common.Card;

namespace NobleBank.Domain.Entities
{
    public class Card : BaseEntity
    {
        public string EncryptedCardNumber { get; private set; } = string.Empty;

        public string Last4Digits { get; private set; } = string.Empty;

        public string CardHolder { get; private set; } = string.Empty;

        public CardType Type { get; private set; }

        public CardStatus Status { get; private set; }

        public decimal Balance { get; private set; }

        public decimal? CreditLimit { get; private set; }

        public string Currency { get; private set; } = Constants.Card.DefaultCurrency;

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
            string plainCardNumber,
            CardType type,
            string userId,
            string createdBy,
            IEncryptionService encryption,
            decimal initialBalance = 0,
            decimal? creditLimit = null)
        {
            return new Card
            {
                CardHolder = cardHolder.ToUpper().Trim(),
                EncryptedCardNumber = encryption.Encrypt(plainCardNumber),
                Last4Digits = plainCardNumber[^4..],
                Type = type,
                Status = CardStatus.Pending,
                Balance = initialBalance,
                CreditLimit = type == CardType.Credit ? creditLimit : null,
                ExpiryDate = DateTime.UtcNow.AddYears(4),
                UserId = userId,
                CreatedBy = createdBy
            };
        }

        public void Block(string reason, string performedBy)
        {
            Status = CardStatus.Blocked;
            UpdatedAt = DateTime.UtcNow;
            LastModifiedBy = performedBy;

            AddDomainEvent(new CardBlockedEvent(Id, UserId, reason, performedBy, DateTime.UtcNow));
        }
    }
}