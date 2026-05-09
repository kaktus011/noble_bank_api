using AutoMapper;
using AutoMapper.QueryableExtensions;
using NobleBank.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NobleBank.Application.Features.Loans.Queries.GetAllLoans;

public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, List<LoanDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllLoansQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LoanDto>> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
    {
        return await _context.Loans
            .Where(l => l.UserId == request.UserId)
            .OrderByDescending(l => l.CreatedAt)
            .ProjectTo<LoanDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
