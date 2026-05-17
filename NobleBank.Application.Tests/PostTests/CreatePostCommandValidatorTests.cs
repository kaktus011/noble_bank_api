using FluentValidation.TestHelper;
using NobleBank.Application.Features.Posts.Commands.CreatePost;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Tests.PostTests
{
    public class CreatePostCommandValidatorTests
    {
        private readonly CreatePostCommandValidator _validator;

        public CreatePostCommandValidatorTests()
        {
            _validator = new CreatePostCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
        {
            // Arrange
            var command = new CreatePostCommand("user-1", "Valid Title", "Valid Body");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyTitle_ShouldHaveError(string title)
        {
            // Arrange
            var command = new CreatePostCommand("user-1", title, "Valid Body");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage(Constants.Requirements.PostTitleRequired);
        }

        [Fact]
        public void Validate_WithTitleTooLong_ShouldHaveError()
        {
            // Arrange
            var command = new CreatePostCommand("user-1", new string('a', 201), "Valid Body");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage(Constants.Requirements.PostTitleMaxLength);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyBody_ShouldHaveError(string body)
        {
            // Arrange
            var command = new CreatePostCommand("user-1", "Valid Title", body);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Body)
                .WithErrorMessage(Constants.Requirements.PostBodyRequired);
        }

        [Fact]
        public void Validate_WithBodyTooLong_ShouldHaveError()
        {
            // Arrange
            var command = new CreatePostCommand("user-1", "Valid Title", new string('a', 5001));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Body)
                .WithErrorMessage(Constants.Requirements.PostBodyMaxLength);
        }
    }
}