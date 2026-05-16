using NobleBank.Domain.Common;

namespace NobleBank.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private init; } = Guid.NewGuid();

    public decimal Amount { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public TransactionsEnum.Type Type { get; private set; }

    public DateTime OccurredAt { get; private set; }

    // Foreign Keys
    public Guid CardId { get; private set; }

    public Card Card { get; private set; } = null!;

    public string PerformedBy { get; private set; } = string.Empty;

    private Transaction() { }

    public static Transaction Create(
        decimal amount,
        string description,
        TransactionsEnum.Type type,
        Card card,
        string performedBy)
    {
        return new Transaction
        {
            Amount = amount,
            Description = description,
            Type = type,
            Card = card,
            CardId = card.Id,
            OccurredAt = DateTime.UtcNow,
            PerformedBy = performedBy
        };
    }
}