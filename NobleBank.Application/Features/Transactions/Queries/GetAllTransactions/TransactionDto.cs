namespace NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;

public record TransactionDto
{
    public Guid Id { get; init; }

    public decimal Amount { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public DateTime OccurredAt { get; init; }

    public Guid CardId { get; init; }

    public string CardLast4 { get; init; } = string.Empty;  // for display
}