using FluentValidation;
using MediatR;
using NobleBank.Application.Common.Behaviours;

namespace NobleBank.Application.Tests
{
    public class BehavioursTests
    {
        [Fact]
        public async Task ValidationBehaviour_WhenNoValidatorsExist_ShouldCallNext()
        {
            // Arrange
            var behaviour = new ValidationBehaviour<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());
            var nextCalled = false;
            RequestHandlerDelegate<string> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            };

            // Act
            var result = await behaviour.Handle(new TestRequest("John"), next, CancellationToken.None);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal("ok", result);
        }

        [Fact]
        public async Task ValidationBehaviour_WhenValidationFails_ShouldThrowValidationException()
        {
            // Arrange
            var validators = new IValidator<TestRequest>[]
            {
                new TestRequestValidator()
            };

            var behaviour = new ValidationBehaviour<TestRequest, string>(validators);
            var nextCalled = false;
            RequestHandlerDelegate<string> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult("should not run");
            };

            // Act
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => behaviour.Handle(new TestRequest(string.Empty), next, CancellationToken.None));

            // Assert
            Assert.False(nextCalled);
            Assert.Single(ex.Errors);
            Assert.Equal("Name", ex.Errors.First().PropertyName);
            Assert.Equal("Name is required.", ex.Errors.First().ErrorMessage);
        }

        private sealed record TestRequest(string Name);

        private sealed class TestRequestValidator : AbstractValidator<TestRequest>
        {
            public TestRequestValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Name is required.");
            }
        }
    }
}