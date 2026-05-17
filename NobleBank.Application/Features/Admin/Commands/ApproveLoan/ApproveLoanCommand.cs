using MediatR;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Admin.Commands.ApproveLoan
{
    public record ApproveLoanCommand(
        [property: JsonIgnore]
        string? AdminUserId,
        Guid LoanId) : IRequest;
}
