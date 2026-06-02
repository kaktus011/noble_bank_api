using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
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
                return new AuthResult(false, null, error);

            ApplicationUser? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return new AuthResult(false, null, "User not found");

            // Another session is active — ask the caller whether to force it out
            if (user.SessionId.HasValue && !request.ForceLogin)
                return new AuthResult(false, null, null, HasActiveSession: true);

            // Stamp a new session (overwrites any stale one when ForceLogin == true)
            user.SessionId = Guid.NewGuid();
            await _context.SaveChangesAsync(cancellationToken);

            string token = await _tokenService.GenerateToken(
                userId!,
                user.Email ?? request.Email,
                user.FullName,
                user.SessionId!.Value);

            return new AuthResult(true, token, null);
        }
    }
}
