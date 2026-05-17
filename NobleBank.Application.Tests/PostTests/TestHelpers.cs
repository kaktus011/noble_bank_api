using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Posts;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Tests.PostTests
{
    internal static class TestHelpers
    {
        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<PostMappingProfile>());

            return configuration.CreateMapper();
        }

        public static TestApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestApplicationDbContext(options);
        }

        public static ApplicationUser CreateUser(string userId, string firstName = "John", string lastName = "Doe", string email = "john.doe@example.com")
        {
            return new ApplicationUser
            {
                Id = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
        }

        public static Post CreatePost(
            Guid id,
            string userId,
            string title = "Test Post",
            string body = "Test Body",
            DateTime? createdAt = null)
        {
            var post = Post.Create(
                title: title,
                body: body,
                userId: userId,
                createdBy: userId);

            SetPrivateProperty(post, nameof(Post.Id), id);
            SetPrivateProperty(post, nameof(Post.CreatedAt), createdAt ?? DateTime.UtcNow);

            return post;
        }

        public static void SetPrivateProperty<T>(T instance, string propertyName, object value)
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
        }
    }
}
