using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NobleBank.API.Middleware;
using NobleBank.Application.Common.Exceptions;

namespace NobleBank.API.Tests
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WhenValidationExceptionOccurs_ShouldReturn400WithErrorsAndTraceId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new ExceptionHandlingMiddleware(_ => throw CreateValidationException(), NullLogger<ExceptionHandlingMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);
            context.Response.Body.Position = 0;
            var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Validation failed", problem!.Title);
            Assert.True(problem.Extensions.ContainsKey("traceId"));
            Assert.True(problem.Extensions.ContainsKey("errors"));
        }

        [Fact]
        public async Task InvokeAsync_WhenUnhandledExceptionOccurs_ShouldReturnGeneric500()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("secret details"), NullLogger<ExceptionHandlingMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);
            context.Response.Body.Position = 0;
            var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("An error occurred", problem!.Title);
            Assert.Equal("An unexpected error occurred. Please contact support with the provided trace identifier.", problem.Detail);
        }

        [Fact]
        public async Task InvokeAsync_WhenNotFoundExceptionOccurs_ShouldReturn404()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new ExceptionHandlingMiddleware(_ => throw new NotFoundException("Card not found"), NullLogger<ExceptionHandlingMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);
            context.Response.Body.Position = 0;
            var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Not Found", problem!.Title);
            Assert.Equal("Card not found", problem.Detail);
        }

        private static ValidationException CreateValidationException()
        {
            return new ValidationException(new[]
            {
                new ValidationFailure("Email", "Email is required."),
                new ValidationFailure("Email", "Email format is invalid.")
            });
        }
    }
}
