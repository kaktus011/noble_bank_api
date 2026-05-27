using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Admin.Queries.GetPendingCards
{
    public class GetPendingCardsQueryHandler : IRequestHandler<GetPendingCardsQuery, List<CardDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetPendingCardsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CardDto>> Handle(GetPendingCardsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Cards
                .Where(c => c.Status == CardEnum.Status.Pending)
                .OrderBy(c => c.CreatedAt)
                .ProjectTo<CardDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
