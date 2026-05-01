using Microsoft.AspNetCore.Mvc;
using NobleBank.Application.Common.Exceptions;
using System.Text.Json;

namespace NobleBank.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);

                (int statusCode, string? title) = ex switch
                {
                    UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                    NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                    _ => (StatusCodes.Status500InternalServerError, "An error occurred")
                };

                string detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred. Please contact support with the provided trace identifier."
                    : ex.Message;

                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the exception handling middleware will not modify the response.");

                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                ProblemDetails problem = new()
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Instance = context.Request.Path
                };

                problem.Extensions["traceId"] = context.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(problem, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
        }
    }
}
