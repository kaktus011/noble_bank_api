using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Admin.Queries.GetPendingLoans
{
    public class GetPendingLoansQueryHandler : IRequestHandler<GetPendingLoansQuery, List<LoanDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetPendingLoansQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<LoanDto>> Handle(GetPendingLoansQuery request, CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Where(l => l.Status == LoansEnum.Status.Pending)
                .OrderBy(l => l.CreatedAt)
                .ProjectTo<LoanDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
