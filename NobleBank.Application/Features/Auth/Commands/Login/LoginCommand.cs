using MediatR;
using NobleBank.Application.Features.Auth.Commands.Register;

namespace NobleBank.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password) : IRequest<AuthResult>;
}