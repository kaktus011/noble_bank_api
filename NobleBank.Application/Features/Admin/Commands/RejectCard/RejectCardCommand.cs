using MediatR;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Admin.Commands.RejectCard
{
    public record RejectCardCommand(
        [property: JsonIgnore]
        string? AdminUserId,
        Guid CardId,
        string Reason) : IRequest;
}
