using MediatR;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;

namespace NobleBank.Application.Features.Loans.Queries.GetLoanById
{
    public record GetLoanByIdQuery(Guid LoanId, string? UserId) : IRequest<LoanDto?>;
}