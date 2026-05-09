using MediatR;

namespace NobleBank.Application.Features.Loans.Queries.GetAllLoans
{
    public record GetAllLoansQuery(string UserId) : IRequest<List<LoanDto>>;
}