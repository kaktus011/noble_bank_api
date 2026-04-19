using AutoMapper;
using MediatR;
using NobleBank.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace NobleBank.Application.Features.Cards.Queries.GetAllCards
{
    public class GetAllCardsQueryHandler : IRequestHandler<GetAllCardsQuery, List<CardDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllCardsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CardDto>> Handle(GetAllCardsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Cards
                .Where(c => c.UserId == request.UserId)
                .OrderByDescending(c => c.CreatedAt)
                .ProjectTo<CardDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}