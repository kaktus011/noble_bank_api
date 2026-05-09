using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.LoanTests
{
    public class GetAllLoansQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnOnlyCurrentUsersLoansInDescendingOrder()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            context.Loans.AddRange(
                TestHelpers.CreateLoan(Guid.NewGuid(), "user-1", 10000m, 9000m, 5.5m, 60, LoansEnum.Type.Personal, LoansEnum.Status.Active, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(5), DateTime.UtcNow.AddDays(-2)),
                TestHelpers.CreateLoan(Guid.NewGuid(), "user-1", 5000m, 4000m, 3.5m, 24, LoansEnum.Type.Auto, LoansEnum.Status.Active, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddYears(2), DateTime.UtcNow.AddDays(-1)),
                TestHelpers.CreateLoan(Guid.NewGuid(), "user-2", 7000m, 6000m, 4.5m, 36, LoansEnum.Type.Student, LoansEnum.Status.Active, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddYears(3), DateTime.UtcNow.AddDays(-3)));
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetAllLoansQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllLoansQuery("user-1");

            // Act
            List<LoanDto> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, loan => Assert.Equal("user-1", context.Loans.Single(l => l.Id == loan.Id).UserId));
            Assert.Contains(result, loan => loan.Amount == 10000m);
            Assert.Contains(result, loan => loan.Amount == 5000m);
            Assert.DoesNotContain(result, loan => loan.Amount == 7000m);
        }
    }
}
