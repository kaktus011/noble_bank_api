using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly IApplicationDbContext _context;

        public RegisterCommandHandler(
            IIdentityService identityService,
            ITokenService tokenService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            (bool success, string? userId, string? error) = await _identityService.RegisterAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName);

            if (!success)
                return new AuthResult(false, null, error);

            ApplicationUser? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return new AuthResult(false, null, "User not found after registration");

            user.SessionId = Guid.NewGuid();
            await _context.SaveChangesAsync(cancellationToken);

            string token = await _tokenService.GenerateToken(
                userId!,
                request.Email,
                $"{request.FirstName} {request.LastName}",
                user.SessionId.Value);

            return new AuthResult(true, token, null);
        }
    }
}
