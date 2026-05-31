using NobleBank.Application.Features.Posts.Queries.GetPostById;

namespace NobleBank.Application.Tests.PostTests
{
    public class GetPostByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithExistingPost_ShouldReturnPostDto()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            context.Users.Add(TestHelpers.CreateUser(userId));

            var postId = Guid.NewGuid();
            context.Posts.Add(TestHelpers.CreatePost(postId, userId, "Test Title", "Test Body"));
            await context.SaveChangesAsync();

            var handler = new GetPostByIdQueryHandler(context, TestHelpers.CreateMapper());

            // Act
            var result = await handler.Handle(new GetPostByIdQuery(postId), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.Id);
            Assert.Equal("Test Title", result.Title);
            Assert.Equal("Test Body", result.Body);
        }

        [Fact]
        public async Task Handle_WithNonExistentPostId_ShouldReturnNull()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            context.Users.Add(TestHelpers.CreateUser("user-1"));
            await context.SaveChangesAsync();

            var handler = new GetPostByIdQueryHandler(context, TestHelpers.CreateMapper());

            // Act
            var result = await handler.Handle(new GetPostByIdQuery(Guid.NewGuid()), CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_WithPostCreatedByDifferentUser_ShouldStillReturnPost()
        {
            // Arrange — posts are global announcements; any authenticated user can read any post
            using var context = TestHelpers.CreateDbContext();
            context.Users.Add(TestHelpers.CreateUser("admin-1"));
            context.Users.Add(TestHelpers.CreateUser("user-1"));

            var postId = Guid.NewGuid();
            context.Posts.Add(TestHelpers.CreatePost(postId, "admin-1"));
            await context.SaveChangesAsync();

            var handler = new GetPostByIdQueryHandler(context, TestHelpers.CreateMapper());

            // Act — user-1 requests a post created by admin-1
            var result = await handler.Handle(new GetPostByIdQuery(postId), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.Id);
        }
    }
}
