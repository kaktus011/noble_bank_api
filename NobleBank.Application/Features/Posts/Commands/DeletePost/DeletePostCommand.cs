using MediatR;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Posts.Commands.DeletePost
{
    public record DeletePostCommand(
        [property: JsonIgnore]
        string? UserId,
        Guid PostId) : IRequest;
}
