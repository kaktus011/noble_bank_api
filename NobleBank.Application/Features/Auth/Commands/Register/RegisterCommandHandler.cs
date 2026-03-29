using MediatR;
using NobleBank.Application.Common.Interfaces;

namespace NobleBank.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;

        public RegisterCommandHandler(IIdentityService identityService, ITokenService tokenService)
        {
            _identityService = identityService;
            _tokenService = tokenService;
        }

        public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            (bool success, string? userId, string? error) = await _identityService.RegisterAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName);

            if (!success)
            {
                return new AuthResult(false, null, error);
            }

            string token = _tokenService.GenerateToken(
                userId,
                request.Email,
                $"{request.FirstName} {request.LastName}");

            return new AuthResult(true, token, null);
        }
    }
}
