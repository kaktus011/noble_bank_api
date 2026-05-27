using MediatR;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;

namespace NobleBank.Application.Features.Admin.Queries.GetPendingLoans
{
    public record GetPendingLoansQuery : IRequest<List<LoanDto>>;
}
