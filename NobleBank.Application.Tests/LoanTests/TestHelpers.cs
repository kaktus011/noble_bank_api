using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Loans;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
using NobleBank.Domain.Interfaces;

namespace NobleBank.Application.Tests.LoanTests
{
    internal static class TestHelpers
    {
        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<LoanMappingProfile>());
            return configuration.CreateMapper();
        }

        public static TestApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestApplicationDbContext(options);
        }

        public static Loan CreateLoan(
            Guid id,
            string userId,
            decimal amount,
            decimal remainingAmount,
            decimal interestRate,
            int termMonths,
            LoansEnum.Type type,
            LoansEnum.Status status,
            DateTime startDate,
            DateTime? endDate,
            DateTime createdAt)
        {
            var loan = Loan.Create(
                amount: amount,
                interestRate: interestRate,
                termMonths: termMonths,
                type: type,
                userId: userId,
                createdBy: userId);

            SetPrivateProperty(loan, nameof(Loan.Id), id);
            SetPrivateProperty(loan, nameof(Loan.RemainingAmount), remainingAmount);
            SetPrivateProperty(loan, nameof(Loan.Status), status);
            SetPrivateProperty(loan, nameof(Loan.StartDate), startDate);
            SetPrivateProperty(loan, nameof(Loan.EndDate), endDate);
            SetPrivateProperty(loan, nameof(Loan.CreatedAt), createdAt);
            SetPrivateProperty(loan, nameof(Loan.UpdatedAt), createdAt);
            SetPrivateProperty(loan, nameof(Loan.LastModifiedBy), userId);

            return loan;
        }

        public static void SetPrivateProperty<T>(T instance, string propertyName, object? value)
        {
            var property = typeof(T).GetProperty(propertyName);
            Assert.NotNull(property);
            property!.SetValue(instance, value);
        }

        internal sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
        {
            public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<Card> Cards => Set<Card>();
            public DbSet<Loan> Loans => Set<Loan>();
            public DbSet<Transaction> Transactions => Set<Transaction>();
            public DbSet<Post> Posts => Set<Post>();
            public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
                => base.SaveChangesAsync(cancellationToken);
        }
    }
}
