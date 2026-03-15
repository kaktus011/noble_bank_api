using NobleBank.Domain.Common;

namespace NobleBank.Domain.Entities
{
	public class Transaction : BaseEntity
	{
		public Guid CardId { get; private set; }

		public Card Card { get; private set; } = null!;

		public decimal Amount { get; private set; }

		public string Currency { get; private set; } = string.Empty;

		public Transactions.Type Type { get; private set; }

		public string Description { get; private set; } = string.Empty;

		public string? ReceiverAccount { get; private set; }

		public string? SenderAccount { get; private set; }

		public string? LastModifiedBy { get; private set; }

		public string? CreatedBy { get; private set; }

        public DateTime OccurredAt { get; set; }	// not in Creat()

        private Transaction() { }

		public static Transaction Create(
			Guid cardId,
			decimal amount,
			string currency,
			Transactions.Type type,
			string description,
			string createdBy,
			string? receiverAccount = null,
			string? senderAccount = null)
		{
			return new Transaction
			{
				CardId = cardId,
				Amount = amount,
				Currency = currency,
				Type = type,
				Description = description,
				CreatedBy = createdBy,
				ReceiverAccount = receiverAccount,
				SenderAccount = senderAccount
			};
		}
	}
}