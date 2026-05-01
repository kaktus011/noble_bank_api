using AutoMapper;
using MediatR;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using Microsoft.EntityFrameworkCore;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Cards.Queries.GetCardById
{
    public class GetCardByIdQueryHandler : IRequestHandler<GetCardByIdQuery, CardDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetCardByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CardDto?> Handle(GetCardByIdQuery request, CancellationToken cancellationToken)
        {
            Card? card = await _context.Cards
                .Where(c => c.Id == request.CardId && c.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            return card is null ? null : _mapper.Map<CardDto>(card);
        }
    }
}