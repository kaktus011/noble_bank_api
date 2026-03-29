using NobleBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NobleBank.Infrastructure.Persistence.Configurations
{
    public class CardConfiguration : IEntityTypeConfiguration<Card>
    {
        public void Configure(EntityTypeBuilder<Card> builder)
        {
            // Таблица
            builder.ToTable("Cards");
            builder.HasKey(c => c.Id);

            // --- Картов номер с Value Converter ---
            builder.Property(c => c.CardNumber)
                .HasColumnName("CardNumber")
                .HasColumnType("nvarchar(500)")
                .IsRequired();

            // --- Основни полета ---
            builder.Property(c => c.Last4Digits)
                .HasColumnType("nchar(4)")
                .IsRequired();

            builder.Property(c => c.CardHolder)
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            builder.Property(c => c.Balance)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(c => c.CreditLimit)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(c => c.Currency)
                .HasColumnType("nvarchar(3)")
                .IsRequired()
                .HasDefaultValue("EUR");

            builder.Property(c => c.ExpiryDate)
                .HasColumnType("datetime2")
                .IsRequired();

            // --- Enums като int ---
            builder.Property(c => c.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(c => c.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(c => c.Brand)
                .HasConversion<int>()
                .IsRequired();

            // --- Audit fields ---
            builder.Property(c => c.CreatedBy)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Property(c => c.LastModifiedBy)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Property(c => c.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            // --- Релация с User ---
            builder.HasOne(c => c.User)
                .WithMany(u => u.Cards)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // не трием карти ако трием user

            // --- Релация с Transactions ---
            builder.HasMany(c => c.Transactions)
                .WithOne(t => t.Card)
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.Cascade);  // транзакциите се трият с картата

            builder.Navigation(c => c.Transactions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // --- Индекси ---
            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Cards_UserId");

            builder.HasIndex(c => c.Last4Digits)
                .HasDatabaseName("IX_Cards_Last4Digits");
        }
    }
}