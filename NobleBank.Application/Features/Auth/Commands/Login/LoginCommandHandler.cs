using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Auth.Commands.Register;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly IApplicationDbContext _context;

        public LoginCommandHandler(
            IIdentityService identityService,
            ITokenService tokenService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            (bool success, string? userId, string? error) = await _identityService.LoginAsync(
                request.Email,
                request.Password);

            if (!success)
            {
                return new AuthResult(false, null, error);
            }

            ApplicationUser? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            string token = _tokenService.GenerateToken(
                userId,
                request.Email,
                user?.FullName ?? string.Empty);

            return new AuthResult(true, token, null);
        }
    }
}
