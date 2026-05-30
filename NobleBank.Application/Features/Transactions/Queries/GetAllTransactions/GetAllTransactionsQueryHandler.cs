using AutoMapper;
using AutoMapper.QueryableExtensions;
using NobleBank.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Transactions.Queries.GetAllTransactions
{
    public class GetAllTransactionsQueryHandler
        : IRequestHandler<GetAllTransactionsQuery, List<TransactionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllTransactionsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TransactionDto>> Handle(
            GetAllTransactionsQuery request,
            CancellationToken cancellationToken)
        {
            IQueryable<Transaction> query = _context.Transactions
                .Where(t => request.UserId == null || t.Card.UserId == request.UserId);

            if (request.CardId.HasValue)
            {
                query = query.Where(t => t.CardId == request.CardId.Value);
            }

            int limit = Math.Clamp(request.Limit ?? 50, 1, 50);

            return await query
                .OrderByDescending(t => t.OccurredAt)
                .Take(limit)
                .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}