using MediatR;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Admin.Commands.RejectLoan
{
    public record RejectLoanCommand(
        [property: JsonIgnore]
        string? AdminUserId,
        Guid LoanId,
        string Reason) : IRequest;
}
