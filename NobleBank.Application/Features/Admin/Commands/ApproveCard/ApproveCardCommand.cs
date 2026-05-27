using MediatR;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Admin.Commands.ApproveCard
{
    public record ApproveCardCommand(
        [property: JsonIgnore]
        string? AdminUserId,
        Guid CardId) : IRequest;
}
