using NobleBank.Application.Features.Posts.Queries.GetAllPosts;

namespace NobleBank.Application.Tests.PostTests
{
    public class GetAllPostsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithUserHavingPosts_ShouldReturnPostsOrderedByDate()
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
            var query = new GetAllPostsQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            // Check ordering
            Assert.Equal("Newest", result[0].Title);
            Assert.Equal("Middle", result[1].Title);
            Assert.Equal("Old", result[2].Title);
        }

        [Fact]
        public async Task Handle_WithUserHavingNoPosts_ShouldReturnEmptyList()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new GetAllPostsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllPostsQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldOnlyReturnPostsForRequestedUser()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId1 = "user-1";
            var userId2 = "user-2";
            context.Users.Add(TestHelpers.CreateUser(userId1));
            context.Users.Add(TestHelpers.CreateUser(userId2));

            context.Posts.Add(TestHelpers.CreatePost(Guid.NewGuid(), userId1));
            context.Posts.Add(TestHelpers.CreatePost(Guid.NewGuid(), userId2));
            context.Posts.Add(TestHelpers.CreatePost(Guid.NewGuid(), userId1));

            await context.SaveChangesAsync();

            var handler = new GetAllPostsQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetAllPostsQuery(userId1);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}