using MediatR;

namespace NobleBank.Application.Features.Auth.Commands.Register
{
    public record RegisterCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName) : IRequest<AuthResult>;

    public record AuthResult(
        bool Success,
        string? Token,
        string? Error);
}
