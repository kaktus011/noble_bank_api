using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Auth.Commands.ClearSession
{
    public class ClearSessionCommandHandler : IRequestHandler<ClearSessionCommand>
    {
        private readonly IApplicationDbContext _context;

        public ClearSessionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ClearSessionCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                return;
            }

            // Only clear the session if the caller's token actually owns the
            // current session. Otherwise a stale token from a previously
            // force-logged-out device would wipe the *new* session — kicking
            // out the device that legitimately took over.
            if (request.SessionId is not null && user.SessionId != request.SessionId)
            {
                return;
            }

            user.SessionId = null;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
