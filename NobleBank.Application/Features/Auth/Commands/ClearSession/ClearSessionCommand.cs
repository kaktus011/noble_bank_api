using MediatR;

namespace NobleBank.Application.Features.Auth.Commands.ClearSession
{
    public record ClearSessionCommand(string UserId) : IRequest;
}
