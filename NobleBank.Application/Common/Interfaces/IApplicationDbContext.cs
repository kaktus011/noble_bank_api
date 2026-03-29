using Microsoft.EntityFrameworkCore;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Card> Cards { get; }

        DbSet<Loan> Loans { get; }

        DbSet<Transaction> Transactions { get; }

        DbSet<Post> Posts { get; }

        DbSet<ApplicationUser> Users { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
