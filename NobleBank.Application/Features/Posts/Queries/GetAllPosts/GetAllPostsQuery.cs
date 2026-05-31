using MediatR;

namespace NobleBank.Application.Features.Posts.Queries.GetAllPosts
{
    public record GetAllPostsQuery : IRequest<List<PostDto>>;
}