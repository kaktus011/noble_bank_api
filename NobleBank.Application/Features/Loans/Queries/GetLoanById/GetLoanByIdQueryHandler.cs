using AutoMapper;
using AutoMapper.QueryableExtensions;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NobleBank.Application.Features.Loans.Queries.GetLoanById;

public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLoanByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LoanDto?> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Loans
            .Where(l => l.Id == request.LoanId && (request.UserId == null || l.UserId == request.UserId))
            .ProjectTo<LoanDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}