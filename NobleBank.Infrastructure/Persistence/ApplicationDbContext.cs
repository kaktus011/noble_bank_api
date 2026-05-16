using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            List<EntityEntry<BaseEntity>> entries = ChangeTracker.Entries<BaseEntity>().ToList();
            DateTime utcNow = DateTime.UtcNow;

            foreach (EntityEntry<BaseEntity> entry in entries)
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

            await DispatchDomainEventsAsync(cancellationToken);

            return result;
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            List<BaseEntity> entities = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            List<INotification> events = entities.SelectMany(e => e.DomainEvents).ToList();
            entities.ForEach(e => e.ClearDomainEvents());

            foreach (var @event in events)
            {
                await _mediator.Publish(@event, cancellationToken);
            }
        }
    }
}
