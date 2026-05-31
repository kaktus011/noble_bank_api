using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.LoanTests
{
    public class LoanMappingProfileTests
    {
        [Fact]
        public void Map_LoanToLoanDto_ShouldMapExpectedValues()
        {
            // Arrange
            var mapper = TestHelpers.CreateMapper();
            var loan = TestHelpers.CreateLoan(
                id: Guid.NewGuid(),
                userId: "user-1",
                amount: 10000m,
                remainingAmount: 7500m,
                interestRate: 5.5m,
                termMonths: 60,
                type: LoansEnum.Type.Personal,
                status: LoansEnum.Status.Active,
                startDate: DateTime.UtcNow.AddDays(-1),
                endDate: DateTime.UtcNow.AddYears(5),
                createdAt: DateTime.UtcNow.AddDays(-2));

            // Act
            var dto = mapper.Map<LoanDto>(loan);

            // Assert
            Assert.Equal(loan.Id, dto.Id);
            Assert.Equal(loan.UserId, dto.UserId);
            Assert.Equal(10000m, dto.Amount);
            Assert.Equal(7500m, dto.RemainingAmount);
            Assert.Equal(5.5m, dto.InterestRate);
            Assert.Equal(60, dto.TermMonths);
            Assert.Equal(loan.MonthlyPayment, dto.MonthlyPayment);
            Assert.Equal("Active", dto.Status);
            Assert.Equal("Personal", dto.Type);
            Assert.Equal(loan.StartDate, dto.StartDate);
            Assert.Equal(loan.EndDate, dto.EndDate);
            Assert.Equal(25m, dto.ProgressPercentage);
        }
    }
}
