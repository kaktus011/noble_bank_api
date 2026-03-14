using static NobleBank.Domain.Common.Loans;

namespace NobleBank.Domain.Entities
{
	public class Loan : BaseEntity
	{
		public string Name { get; private set; } = string.Empty;

		public decimal OutstandingAmount { get; private set; }

		public decimal InterestRate { get; private set; }

		public LoanStatus Status { get; private set; }

		public string UserId { get; private set; } = string.Empty;

		public ApplicationUser User { get; private set; } = null!;

		public string? LastModifiedBy { get; private set; }

		public string? CreatedBy { get; private set; }

		private Loan() { }

		public static Loan Create(
			string name,
			decimal outstandingAmount,
			decimal interestRate,
			string userId,
			string createdBy)
		{
			return new Loan
			{
				Name = name,
				OutstandingAmount = outstandingAmount,
				InterestRate = interestRate,
				Status = LoanStatus.Active,
				UserId = userId,
				CreatedBy = createdBy
			};
		}
	}
}