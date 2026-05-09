using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Features.Loans.Commands.RequestLoan;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Tests.LoanTests
{
    public class RequestLoanCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenUserExists_ShouldCreateAndReturnLoanDto()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            context.Users.Add(new ApplicationUser
            {
                Id = "user-1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new RequestLoanCommandHandler(context, TestHelpers.CreateMapper());
            var command = new RequestLoanCommand("user-1", 10000m, 60, LoansEnum.Type.Personal);

            // Act
            LoanDto result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(10000m, result.Amount);
            Assert.Equal(10000m, result.RemainingAmount);
            Assert.Equal(0m, result.ProgressPercentage);
            Assert.Equal("Active", result.Status);
            Assert.Equal("Personal", result.Type);
            Assert.Equal("John Doe", context.Users.Single(u => u.Id == "user-1").FullName);
        }

        [Fact]
        public async Task Handle_WhenUserIsMissing_ShouldThrowNotFoundException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var handler = new RequestLoanCommandHandler(context, TestHelpers.CreateMapper());
            var command = new RequestLoanCommand("missing-user", 10000m, 60, LoansEnum.Type.Personal);

            // Act
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal("User 'missing-user' was not found.", ex.Message);
        }

        [Fact]
        public async Task Handle_WhenUserIdIsMissing_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var context = TestHelpers.CreateDbContext();
            var handler = new RequestLoanCommandHandler(context, TestHelpers.CreateMapper());
            var command = new RequestLoanCommand(null, 10000m, 60, LoansEnum.Type.Personal);

            // Act
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal("User ID is required", ex.Message);
        }
    }
}
