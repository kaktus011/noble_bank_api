// Domain/Entities/Loan.cs
using NobleBank.Domain.Common;

namespace NobleBank.Domain.Entities
{
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

        public string? RejectionReason { get; private set; }

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
            ValidateLoan(amount, interestRate, termMonths);

            decimal monthlyPayment = CalculateMonthlyPayment(amount, interestRate, termMonths);

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
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(string reason, string performedBy)
        {
            Status = LoansEnum.Status.Rejected;
            RejectionReason = reason;
            UpdatedAt = DateTime.UtcNow;
            LastModifiedBy = performedBy;
        }

        public Result<decimal> MakePayment(decimal amount, string performedBy)
        {
            if (Status != LoansEnum.Status.Active)
            {
                return Result<decimal>.Failure("Loan is not active.");
            }

            if (amount <= 0)
            {
                return Result<decimal>.Failure("Payment amount must be positive.");
            }

            if (amount > RemainingAmount)
            {
                amount = RemainingAmount;  // overpayment = exact remaining
            }

            RemainingAmount -= amount;
            RemainingAmount = Math.Round(RemainingAmount, 2, MidpointRounding.AwayFromZero);
            UpdatedAt = DateTime.UtcNow;
            LastModifiedBy = performedBy;

            if (RemainingAmount <= 0m)
            {
                RemainingAmount = 0m;
                Status = LoansEnum.Status.Closed;
                EndDate = DateTime.UtcNow;
            }

            return Result<decimal>.Success(RemainingAmount);
        }

        private static void ValidateLoan(decimal amount, decimal interestRate, int termMonths)
        {
            if (amount <= 0)
            {
                throw new DomainException(Constants.Requirements.LoanMinAmount);
            }

            if (amount > 100000m)
            {
                throw new DomainException(Constants.Requirements.LoanMaxAmount);
            }

            if (interestRate < 0)
            {
                throw new DomainException(Constants.Requirements.LoanInterestRate);
            }

            if (termMonths < 1)
            {
                throw new DomainException(Constants.Requirements.LoanMinTerm);
            }

            if (termMonths > 360)
            {
                throw new DomainException(Constants.Requirements.LoanMaxTerm);
            }
        }

        private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
        {
            if (months <= 0)
            {
                throw new DomainException(Constants.Requirements.LoanMinTerm);
            }

            if (annualRate == 0)
            {
                return principal / months;
            }

            decimal monthlyRate = annualRate / 100 / 12;
            decimal payment = principal * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), months))
                          / ((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1);

            return Math.Round(payment, 2);
        }
    }
}