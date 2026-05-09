namespace NobleBank.Application.Features.Loans.Queries.GetAllLoans;

public record LoanDto
{
    public Guid Id { get; init; }

    public decimal Amount { get; init; }

    public decimal RemainingAmount { get; init; }

    public decimal InterestRate { get; init; }

    public int TermMonths { get; init; }

    public decimal MonthlyPayment { get; init; }

    public string Status { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    public decimal ProgressPercentage { get; init; }  // (Amount - Remaining) / Amount * 100
}