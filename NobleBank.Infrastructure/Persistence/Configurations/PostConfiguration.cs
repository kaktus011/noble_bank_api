using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NobleBank.Domain.Entities;

namespace NobleBank.Infrastructure.Persistence.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Posts");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            builder.Property(p => p.Body)
                .HasColumnType("nvarchar(500)")
                .IsRequired();

            builder.Property(p => p.CreatedBy)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Property(p => p.LastModifiedBy)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            // relation with User
            builder.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // user is deleted -> his posts are auto deleted

            // indexing
            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("IX_Posts_UserId");
        }
    }
}
