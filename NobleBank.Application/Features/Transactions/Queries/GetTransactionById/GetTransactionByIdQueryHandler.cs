using AutoMapper;
using AutoMapper.QueryableExtensions;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NobleBank.Application.Features.Transactions.Queries.GetTransactionById
{
    public class GetTransactionByIdQueryHandler
        : IRequestHandler<GetTransactionByIdQuery, TransactionDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetTransactionByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TransactionDto?> Handle(
            GetTransactionByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.Transactions
                .Where(t => t.Id == request.TransactionId && t.Card.UserId == request.UserId)
                .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}