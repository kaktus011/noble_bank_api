using NobleBank.Application.Features.Admin.Queries.GetPendingLoans;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.LoanTests
{
    public class GetPendingLoansQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnOnlyPendingLoansInCreatedAtAscendingOrder()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var createdAt1 = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var createdAt2 = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc);
            var createdAt3 = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc);
            var createdAt4 = new DateTime(2024, 1, 4, 10, 0, 0, DateTimeKind.Utc);
            var createdAt5 = new DateTime(2024, 1, 5, 10, 0, 0, DateTimeKind.Utc);

            var pendingOldest = TestHelpers.CreateLoan(Guid.NewGuid(), "user-1", 10000m, 9000m, 5.5m, 60, LoansEnum.Type.Personal, LoansEnum.Status.Pending, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddYears(5), createdAt1);
            var activeLoan = TestHelpers.CreateLoan(Guid.NewGuid(), "user-2", 5000m, 4000m, 3.5m, 24, LoansEnum.Type.Auto, LoansEnum.Status.Active, DateTime.UtcNow.AddDays(-4), DateTime.UtcNow.AddYears(2), createdAt2);
            var pendingMiddle = TestHelpers.CreateLoan(Guid.NewGuid(), "user-3", 7000m, 6000m, 4.5m, 36, LoansEnum.Type.Student, LoansEnum.Status.Pending, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddYears(3), createdAt3);
            var pendingNewest = TestHelpers.CreateLoan(Guid.NewGuid(), "user-4", 8000m, 7500m, 4.0m, 48, LoansEnum.Type.Mortgage, LoansEnum.Status.Pending, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddYears(4), createdAt4);
            var rejectedLoan = TestHelpers.CreateLoan(Guid.NewGuid(), "user-5", 6000m, 5500m, 4.2m, 36, LoansEnum.Type.Personal, LoansEnum.Status.Rejected, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(3), createdAt5);

            context.Loans.AddRange(pendingOldest, activeLoan, pendingMiddle, pendingNewest, rejectedLoan);
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetPendingLoansQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetPendingLoansQuery();

            // Act
            List<LoanDto> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, loan => Assert.Equal(LoansEnum.Status.Pending, context.Loans.Single(l => l.Id == loan.Id).Status));
            Assert.Equal(new[] { pendingOldest.Id, pendingMiddle.Id, pendingNewest.Id }, result.Select(loan => loan.Id).ToArray());
            Assert.DoesNotContain(result, loan => context.Loans.Single(l => l.Id == loan.Id).Status != LoansEnum.Status.Pending);
        }
    }
}
