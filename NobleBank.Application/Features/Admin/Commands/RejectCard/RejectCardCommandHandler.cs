using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Admin.Commands.RejectCard
{
    public class RejectCardCommandHandler : IRequestHandler<RejectCardCommand>
    {
        private readonly IApplicationDbContext _context;

        public RejectCardCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(RejectCardCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.AdminUserId))
            {
                throw new UnauthorizedAccessException(Constants.Exceptions.UserNotFound);
            }

            Card? card = await _context.Cards
                .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

            if (card is null)
            {
                throw new Exception(Constants.Exceptions.CardNotFound);
            }

            if (card.Status != CardEnum.Status.Pending)
            {
                throw new Exception($"Cannot reject card in {card.Status} status");
            }

            card.Reject(request.Reason, request.AdminUserId);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
