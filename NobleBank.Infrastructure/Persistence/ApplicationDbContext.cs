using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Entities;
using NobleBank.Domain.Interfaces;
using System.Reflection;

namespace NobleBank.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        private readonly IMediator _mediator;
        private readonly IEncryptionService _encryption;

        public DbSet<Card> Cards => Set<Card>();

        public DbSet<Loan> Loans => Set<Loan>();

        public DbSet<Transaction> Transactions => Set<Transaction>();

        public DbSet<Post> Posts => Set<Post>();

        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IMediator mediator,
            IEncryptionService encryption) : base(options) 
        {
            _mediator = mediator;
            _encryption = encryption;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<Card>()
           .Property(c => c.CardNumber)
           .HasConversion(
               plain => _encryption.Encrypt(plain),
               stored => _encryption.Decrypt(stored));

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            DateTime utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
            }

            int result = await base.SaveChangesAsync(cancellationToken);

            await DispatchDomainEventsAsync();

            return result;
        }

        private async Task DispatchDomainEventsAsync()
        {
            var entities = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var events = entities.SelectMany(e => e.DomainEvents).ToList();
            entities.ForEach(e => e.ClearDomainEvents());

            foreach (var @event in events)
                await _mediator.Publish(@event);
        }
    }
}
