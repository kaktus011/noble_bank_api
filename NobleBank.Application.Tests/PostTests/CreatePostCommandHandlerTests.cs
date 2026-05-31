using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Features.Posts.Commands.CreatePost;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.PostTests
{
    public class CreatePostCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreatePostAndReturnDto()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            var user = TestHelpers.CreateUser(userId);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var handler = new CreatePostCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreatePostCommand(userId, "Test Title", "Test Body");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(command.Title, result.Title);
            Assert.Equal(command.Body, result.Body);

            var postInDb = await context.Posts.FirstOrDefaultAsync(p => p.Id == result.Id);
            Assert.NotNull(postInDb);
            Assert.Equal(command.Title, postInDb.Title);
            Assert.Equal(command.Body, postInDb.Body);
            Assert.Equal(userId, postInDb.UserId);
            Assert.Equal(userId, postInDb.CreatedBy);
        }

        [Fact]
        public async Task Handle_WithEmptyUserId_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var handler = new CreatePostCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreatePostCommand(string.Empty, "Test Title", "Test Body");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal(Constants.Requirements.UserIdRequired, exception.Message);
        }

        [Fact]
        public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var handler = new CreatePostCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreatePostCommand("non-existent-user", "Test Title", "Test Body");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("User not found", exception.Message);
        }

        [Fact]
        public async Task Handle_WithHtmlInContent_ShouldStripHtmlTagsBeforeSaving()
        {
            // Arrange
            using var context = TestHelpers.CreateDbContext();
            var userId = "user-1";
            context.Users.Add(TestHelpers.CreateUser(userId));
            await context.SaveChangesAsync();

            var handler = new CreatePostCommandHandler(context, TestHelpers.CreateMapper());
            var command = new CreatePostCommand(
                userId,
                "Hello <b>World</b>",
                "Click <a href='x'>here</a><script>alert('xss')</script>"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Hello World", result.Title);
            Assert.Equal("Click herealert('xss')", result.Body);

            var postInDb = await context.Posts.FindAsync(result.Id);
            Assert.NotNull(postInDb);
            Assert.Equal("Hello World", postInDb.Title);
            Assert.Equal("Click herealert('xss')", postInDb.Body);
        }
    }
}