using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NobleBank.Domain.Entities;

namespace NobleBank.Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnType("nvarchar(250)")
                .IsRequired(false);

            builder.Property(t => t.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.OccurredAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasOne(t => t.Card)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.CardId)
                .HasDatabaseName("IX_Transactions_CardId");

            builder.HasIndex(t => t.OccurredAt)
                .HasDatabaseName("IX_Transactions_OccurredAt");
        }
    }
}