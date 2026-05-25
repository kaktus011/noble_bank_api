using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Admin.Commands.ApproveCard
{
    public class ApproveCardCommandHandler : IRequestHandler<ApproveCardCommand>
    {
        private readonly IApplicationDbContext _context;

        public ApproveCardCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ApproveCardCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.AdminUserId))
            {
                throw new UnauthorizedAccessException(Constants.Exceptions.UserNotFound);
            }

            Card? card = await _context.Cards
                .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

            if (card is null)
            {
                throw new NotFoundException(Constants.Exceptions.CardNotFound);
            }

            if (card.Status != CardEnum.Status.Pending)
            {
                throw new DomainException($"Cannot approve card in {card.Status} status");
            }

            card.Activate(request.AdminUserId);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
