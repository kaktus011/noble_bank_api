using AutoMapper;
using NobleBank.Application.Features.Posts.Queries.GetAllPosts;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Posts
{
    public class PostMappingProfile : Profile
    {
        public PostMappingProfile()
        {
            CreateMap<Post, PostDto>();
        }
    }
}