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
                .Include(t => t.Card)
                .Where(t => t.Card.UserId == request.UserId);

            if (request.CardId.HasValue)
            {
                query = query.Where(t => t.CardId == request.CardId.Value);
            }

            return await query
                .OrderByDescending(t => t.OccurredAt)
                .Take(request.Limit ?? 50)
                .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}