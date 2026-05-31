using NobleBank.API.Controllers;
using NobleBank.Application.Features.Posts.Commands.CreatePost;
using NobleBank.Application.Features.Posts.Commands.DeletePost;
using NobleBank.Application.Features.Posts.Queries.GetAllPosts;
using NobleBank.Application.Features.Posts.Queries.GetPostById;
using Microsoft.AspNetCore.Mvc;
using NobleBank.Domain.Common;

namespace NobleBank.API.Tests
{
    public class PostsControllerTests
    {
        private PostsController CreateController(string? userId = "user-1", object? response = null)
        {
            var mediator = new TestHelpers.RecordingMediator(_ => response);
            return new PostsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWithPosts()
        {
            // Arrange
            var expectedPosts = new List<PostDto>
            {
                new() { Id = Guid.NewGuid(), Title = "Post 1", Body = "Body 1", CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Title = "Post 2", Body = "Body 2", CreatedAt = DateTime.UtcNow }
            };

            var mediator = new TestHelpers.RecordingMediator(_ => expectedPosts);
            var controller = new PostsController(mediator) { ControllerContext = TestHelpers.CreateControllerContext("user-1") };

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualPosts = Assert.IsAssignableFrom<List<PostDto>>(okResult.Value);
            Assert.Equal(2, actualPosts.Count);

            var request = Assert.Single(mediator.Requests);
            Assert.IsType<GetAllPostsQuery>(request);
        }

        [Fact]
        public async Task GetById_WhenPostExists_ShouldReturnOk()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var expectedPost = new PostDto { Id = postId, Title = "Post 1", Body = "Body 1" };

            var mediator = new TestHelpers.RecordingMediator(_ => expectedPost);
            var controller = new PostsController(mediator) { ControllerContext = TestHelpers.CreateControllerContext("user-1") };

            // Act
            var result = await controller.GetById(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualPost = Assert.IsType<PostDto>(okResult.Value);
            Assert.Equal(postId, actualPost.Id);

            var request = Assert.Single(mediator.Requests);
            var query = Assert.IsType<GetPostByIdQuery>(request);
            Assert.Equal(postId, query.PostId);
        }

        [Fact]
        public async Task GetById_WhenPostNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator(_ => null);
            var controller = new PostsController(mediator) { ControllerContext = TestHelpers.CreateControllerContext("user-1") };

            // Act
            var result = await controller.GetById(Guid.NewGuid());

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var value = notFoundResult.Value;
            Assert.NotNull(value);

            var propertyInfo = value.GetType().GetProperty("error");
            Assert.NotNull(propertyInfo);
            var errorValue = propertyInfo.GetValue(value) as string;
            Assert.Equal(Constants.Exceptions.PostNotFound, errorValue);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var userId = "user-1";
            var command = new CreatePostCommand(string.Empty, "New Post", "New Body");
            var newPostId = Guid.NewGuid();
            var expectedPost = new PostDto { Id = newPostId, Title = command.Title, Body = command.Body };

            var mediator = new TestHelpers.RecordingMediator(request =>
            {
                var cmd = request as CreatePostCommand;
                Assert.NotNull(cmd);
                Assert.Equal(userId, cmd.UserId);
                return expectedPost;
            });

            var controller = new PostsController(mediator) { ControllerContext = TestHelpers.CreateControllerContext(userId) };

            // Act
            var result = await controller.Create(command);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PostsController.GetById), createdAtResult.ActionName);
            var routeValues = createdAtResult.RouteValues;
            Assert.NotNull(routeValues);
            Assert.True(routeValues.ContainsKey("id"));
            Assert.Equal(newPostId, routeValues["id"]);

            var actualPost = Assert.IsType<PostDto>(createdAtResult.Value);
            Assert.Equal(newPostId, actualPost.Id);
        }

        [Fact]
        public async Task Create_WhenUserIdIsNullOrWhiteSpace_ShouldReturnUnauthorized()
        {
            // Arrange
            var controller = CreateController(string.Empty);

            // Act
            var result = await controller.Create(new CreatePostCommand(string.Empty, "title", "body"));

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent()
        {
            // Arrange
            var userId = "user-1";
            var postId = Guid.NewGuid();

            var mediator = new TestHelpers.RecordingMediator(_ => MediatR.Unit.Value);
            var controller = new PostsController(mediator) { ControllerContext = TestHelpers.CreateControllerContext(userId) };

            // Act
            var result = await controller.Delete(postId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var request = Assert.Single(mediator.Requests);
            var command = Assert.IsType<DeletePostCommand>(request);
            Assert.Equal(postId, command.PostId);
            Assert.Equal(userId, command.UserId);
        }

        [Fact]
        public async Task Delete_WhenUserIdIsNullOrWhiteSpace_ShouldReturnUnauthorized()
        {
            // Arrange
            var controller = CreateController(string.Empty);

            // Act
            var result = await controller.Delete(Guid.NewGuid());

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
