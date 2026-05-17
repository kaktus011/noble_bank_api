using MediatR;

namespace NobleBank.Application.Features.Posts.Queries.GetAllPosts
{
    public record GetAllPostsQuery(string UserId) : IRequest<List<PostDto>>;
}