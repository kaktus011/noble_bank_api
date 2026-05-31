using MediatR;
using NobleBank.Application.Features.Posts.Queries.GetAllPosts;

namespace NobleBank.Application.Features.Posts.Queries.GetPostById
{
    public record GetPostByIdQuery(Guid PostId) : IRequest<PostDto?>;
}
