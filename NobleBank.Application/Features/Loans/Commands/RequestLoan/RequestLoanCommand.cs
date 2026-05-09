using MediatR;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Common;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Loans.Commands.RequestLoan
{
    public record RequestLoanCommand(
        [property: JsonIgnore]
    string? UserId,

        decimal Amount,
        int TermMonths,
        LoansEnum.Type Type) : IRequest<LoanDto>;
}
