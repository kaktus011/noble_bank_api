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
            var user = TestHelpers.CreateUser(userId);
            context.Users.Add(user);

            var postId = Guid.NewGuid();
            var post = TestHelpers.CreatePost(postId, userId, "Test Title", "Test Body");
            context.Posts.Add(post);
            await context.SaveChangesAsync();

            var handler = new GetPostByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetPostByIdQuery(postId, userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

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
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new GetPostByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetPostByIdQuery(Guid.NewGuid(), userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_WithPostBelongingToAnotherUser_ShouldReturnNull()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId1 = "user-1";
            var userId2 = "user-2";
            context.Users.Add(TestHelpers.CreateUser(userId1));
            context.Users.Add(TestHelpers.CreateUser(userId2));

            var postId = Guid.NewGuid();
            var post = TestHelpers.CreatePost(postId, userId2); // Post belongs to user2
            context.Posts.Add(post);
            await context.SaveChangesAsync();

            var handler = new GetPostByIdQueryHandler(context, TestHelpers.CreateMapper());
            var query = new GetPostByIdQuery(postId, userId1); // user1 requests the post

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}