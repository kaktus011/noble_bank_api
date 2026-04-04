using MediatR;

namespace NobleBank.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password) : IRequest<AuthResult>;
}