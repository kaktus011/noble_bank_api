using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Application.Features.Loans.Queries.GetLoanById;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.LoanTests
{
    public class GetLoanByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenLoanExistsForUser_ShouldReturnLoanDto()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var loan = TestHelpers.CreateLoan(Guid.NewGuid(), "user-1", 10000m, 7500m, 5.5m, 60, LoansEnum.Type.Personal, LoansEnum.Status.Active, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(5), DateTime.UtcNow.AddDays(-2));
            context.Loans.Add(loan);
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetLoanByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetLoanByIdQuery(loan.Id, "user-1");

            // Act
            LoanDto? result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loan.Id, result!.Id);
            Assert.Equal(10000m, result.Amount);
            Assert.Equal("Active", result.Status);
            Assert.Equal("Personal", result.Type);
        }

        [Fact]
        public async Task Handle_WhenLoanDoesNotMatchUser_ShouldReturnNull()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var loan = TestHelpers.CreateLoan(Guid.NewGuid(), "user-1", 10000m, 7500m, 5.5m, 60, LoansEnum.Type.Personal, LoansEnum.Status.Active, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(5), DateTime.UtcNow.AddDays(-2));
            context.Loans.Add(loan);
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetLoanByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetLoanByIdQuery(loan.Id, "user-2");

            // Act
            LoanDto? result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
