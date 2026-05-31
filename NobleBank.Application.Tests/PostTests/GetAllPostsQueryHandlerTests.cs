using NobleBank.Application.Features.Posts.Queries.GetAllPosts;

namespace NobleBank.Application.Tests.PostTests
{
    public class GetAllPostsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithPostsInDb_ShouldReturnPostsOrderedByDateDescending()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            context.Users.Add(user);

            var now = DateTime.UtcNow;
            var olderPost = TestHelpers.CreatePost(Guid.NewGuid(), userId, "Old", "Body", now.AddDays(-2));
            var newestPost = TestHelpers.CreatePost(Guid.NewGuid(), userId, "Newest", "Body", now);
            var middlePost = TestHelpers.CreatePost(Guid.NewGuid(), userId, "Middle", "Body", now.AddDays(-1));

            context.Posts.AddRange(olderPost, newestPost, middlePost);
            await context.SaveChangesAsync();

            var handler = new GetAllPostsQueryHandler(context, TestHelpers.CreateMapper());

            // Act
            var result = await handler.Handle(new GetAllPostsQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Newest", result[0].Title);
            Assert.Equal("Middle", result[1].Title);
            Assert.Equal("Old", result[2].Title);
        }

        [Fact]
        public async Task Handle_WithNoPostsInDb_ShouldReturnEmptyList()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var user = TestHelpers.CreateUser("user-1");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new GetAllPostsQueryHandler(context, TestHelpers.CreateMapper());

            // Act
            var result = await handler.Handle(new GetAllPostsQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllPostsRegardlessOfAuthor()
        {
            // Arrange — posts created by two different users (e.g. two admins)
            using var context = TestHelpers.CreateDbContext();
            context.Users.Add(TestHelpers.CreateUser("user-1"));
            context.Users.Add(TestHelpers.CreateUser("user-2"));
            context.Posts.Add(TestHelpers.CreatePost(Guid.NewGuid(), "user-1"));
            context.Posts.Add(TestHelpers.CreatePost(Guid.NewGuid(), "user-2"));
            context.Posts.Add(TestHelpers.CreatePost(Guid.NewGuid(), "user-1"));
            await context.SaveChangesAsync();

            var handler = new GetAllPostsQueryHandler(context, TestHelpers.CreateMapper());

            // Act
            var result = await handler.Handle(new GetAllPostsQuery(), CancellationToken.None);

            // Assert — all 3 posts are visible (posts are global announcements)
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }
    }
}
