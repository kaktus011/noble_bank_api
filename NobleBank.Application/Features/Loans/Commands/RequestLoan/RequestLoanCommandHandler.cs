using AutoMapper;
using MediatR;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Loans.Commands.RequestLoan;

public class RequestLoanCommandHandler : IRequestHandler<RequestLoanCommand, LoanDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RequestLoanCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LoanDto> Handle(RequestLoanCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
            throw new UnauthorizedAccessException("User ID is required");

        // Interest rates by type (simplified)
        var interestRate = request.Type switch
        {
            LoansEnum.Type.Personal => 8.5m,
            LoansEnum.Type.Mortgage => 3.5m,
            LoansEnum.Type.Auto => 5.5m,
            LoansEnum.Type.Student => 4.0m,
            _ => 7.0m
        };

        var loan = Loan.Create(
            amount: request.Amount,
            interestRate: interestRate,
            termMonths: request.TermMonths,
            type: request.Type,
            userId: request.UserId,
            createdBy: request.UserId
        );

        // Auto-approve for demo purposes ONLY
        loan.Approve();

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LoanDto>(loan);
    }
}