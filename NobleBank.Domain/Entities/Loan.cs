using static NobleBank.Domain.Common.Loans;

namespace NobleBank.Domain.Entities
{
	public class Loan : BaseEntity
	{
		public string Name { get; private set; } = string.Empty;

		public decimal InterestRate { get; private set; }

		public Status Status { get; private set; }

		public string UserId { get; private set; } = string.Empty;

		public ApplicationUser User { get; private set; } = null!;

		public string? LastModifiedBy { get; private set; }

		public string? CreatedBy { get; private set; }

        public decimal Amount { get; private set; }
        
		public decimal RemainingAmount { get; set; }

        private Loan() { }

		public static Loan Create(
			string name,
			decimal amount,
			decimal interestRate,
			string userId,
			string createdBy)
		{
			return new Loan
			{
				Name = name,
				Amount = amount,
				RemainingAmount = amount,
				InterestRate = interestRate,
				Status = Status.Active,
				UserId = userId,
				CreatedBy = createdBy
			};
		}
	}
}