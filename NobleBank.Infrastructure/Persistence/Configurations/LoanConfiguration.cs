using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NobleBank.Domain.Entities;

namespace NobleBank.Infrastructure.Persistence.Configurations;

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

        builder.Property(l => l.MonthlyPayment)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.TermMonths)
            .IsRequired();

        builder.Property(l => l.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(l => l.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(l => l.StartDate)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(l => l.EndDate)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(l => l.CreatedBy)
            .HasColumnType("nvarchar(450)")
            .IsRequired(false);

        builder.Property(l => l.LastModifiedBy)
            .HasColumnType("nvarchar(450)")
            .IsRequired(false);

        builder.Property(l => l.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnType("datetime2")
            .IsRequired(false);

        // relation with User
        builder.HasOne(l => l.User)
            .WithMany(u => u.Loans)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // index
        builder.HasIndex(l => l.UserId)
            .HasDatabaseName("IX_Loans_UserId");
    }
}