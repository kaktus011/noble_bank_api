// Domain/Entities/Loan.cs
using NobleBank.Domain.Common;

namespace NobleBank.Domain.Entities;

public class Loan : BaseEntity
{
    public decimal Amount { get; private set; }
    public decimal RemainingAmount { get; private set; }
    public decimal InterestRate { get; private set; }  // 4.5 = 4.5%
    public int TermMonths { get; private set; }
    public decimal MonthlyPayment { get; private set; }

    public LoansEnum.Status Status { get; private set; }
    public LoansEnum.Type Type { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public string UserId { get; private set; } = string.Empty;
    public ApplicationUser User { get; private set; } = null!;

    public string? CreatedBy { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private Loan() { }

    public static Loan Create(
        decimal amount,
        decimal interestRate,
        int termMonths,
        LoansEnum.Type type,
        string userId,
        string createdBy)
    {
        var monthlyPayment = CalculateMonthlyPayment(amount, interestRate, termMonths);

        return new Loan
        {
            Amount = amount,
            RemainingAmount = amount,
            InterestRate = interestRate,
            TermMonths = termMonths,
            MonthlyPayment = monthlyPayment,
            Type = type,
            Status = LoansEnum.Status.Pending,
            StartDate = DateTime.UtcNow,
            UserId = userId,
            CreatedBy = createdBy
        };
    }

    public void Approve()
    {
        Status = LoansEnum.Status.Active;
        StartDate = DateTime.UtcNow;
        EndDate = StartDate.AddMonths(TermMonths);
    }

    public void Reject(string reason)
    {
        Status = LoansEnum.Status.Rejected;
    }

    public Result<decimal> MakePayment(decimal amount, string performedBy)
    {
        if (Status != LoansEnum.Status.Active)
            return Result<decimal>.Failure("Loan is not active.");

        if (amount <= 0)
            return Result<decimal>.Failure("Payment amount must be positive.");

        if (amount > RemainingAmount)
            amount = RemainingAmount;  // overpayment = exact remaining

        RemainingAmount -= amount;
        UpdatedAt = DateTime.UtcNow;
        LastModifiedBy = performedBy;

        if (RemainingAmount == 0)
        {
            Status = LoansEnum.Status.Closed;
            EndDate = DateTime.UtcNow;
        }

        return Result<decimal>.Success(RemainingAmount);
    }

    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
    {
        if (annualRate == 0)
            return principal / months;

        var monthlyRate = annualRate / 100 / 12;
        var payment = principal * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), months))
                      / ((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1);

        return Math.Round(payment, 2);
    }
}