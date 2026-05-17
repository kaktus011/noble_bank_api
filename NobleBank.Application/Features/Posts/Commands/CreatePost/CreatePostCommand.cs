using MediatR;
using NobleBank.Application.Features.Posts.Queries.GetAllPosts;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Posts.Commands.CreatePost
{
    public record CreatePostCommand(
        [property: JsonIgnore]
        string? UserId,
        string Title,
        string Body) : IRequest<PostDto>;
}
