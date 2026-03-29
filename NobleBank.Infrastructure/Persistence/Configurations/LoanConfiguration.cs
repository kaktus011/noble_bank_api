using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NobleBank.Domain.Entities;

namespace NobleBank.Infrastructure.Persistence.Configurations
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.ToTable("Loans");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(l => l.RemainingAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(l => l.InterestRate)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(l => l.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne(l => l.User)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(l => l.UserId)
                .HasDatabaseName("IX_Loans_UserId");
        }
    }
}
