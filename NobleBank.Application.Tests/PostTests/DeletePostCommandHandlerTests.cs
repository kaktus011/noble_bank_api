using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Features.Posts.Commands.DeletePost;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.PostTests
{
    public class DeletePostCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WithValidCommand_ShouldDeletePost()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            context.Users.Add(user);

            var postId = Guid.NewGuid();
            var post = TestHelpers.CreatePost(postId, userId);
            context.Posts.Add(post);

            await context.SaveChangesAsync();

            var handler = new DeletePostCommandHandler(context);
            var command = new DeletePostCommand(userId, postId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var postInDb = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            Assert.Null(postInDb);
        }

        [Fact]
        public async Task Handle_WithEmptyUserId_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var handler = new DeletePostCommandHandler(context);
            var command = new DeletePostCommand(string.Empty, Guid.NewGuid());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal(Constants.Requirements.UserIdRequired, exception.Message);
        }

        [Fact]
        public async Task Handle_WithNonExistentPostId_ShouldThrowException()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new DeletePostCommandHandler(context);
            var command = new DeletePostCommand(userId, Guid.NewGuid());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal(Constants.Exceptions.PostNotFound, exception.Message);
        }

        [Fact]
        public async Task Handle_WithPostBelongingToAnotherUser_ShouldThrowException()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId1 = "user-1";
            var userId2 = "user-2";
            var user1 = TestHelpers.CreateUser(userId1);
            var user2 = TestHelpers.CreateUser(userId2);
            context.Users.Add(user1);
            context.Users.Add(user2);

            var postId = Guid.NewGuid();
            var post = TestHelpers.CreatePost(postId, userId2); // belongs to user 2
            context.Posts.Add(post);

            await context.SaveChangesAsync();

            var handler = new DeletePostCommandHandler(context);
            var command = new DeletePostCommand(userId1, postId); // user 1 tries to delete

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal(Constants.Exceptions.PostNotFound, exception.Message);

            // verify it wasn't deleted
            var postInDb = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            Assert.NotNull(postInDb);
        }
    }
}