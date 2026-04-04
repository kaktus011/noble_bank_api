using MediatR;

namespace NobleBank.Application.Features.Auth.Commands.Register
{
    public record RegisterCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName) : IRequest<AuthResult>;
}
