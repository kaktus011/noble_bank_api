using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Posts.Queries.GetAllPosts;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CreatePostCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new UnauthorizedAccessException(Constants.Requirements.UserIdRequired);
            }

            bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);

            if (!userExists)
            {
                throw new NotFoundException("User not found");
            }

            Post post = Post.Create(
                title: request.Title,
                body: request.Body,
                userId: request.UserId,
                createdBy: request.UserId
            );

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<PostDto>(post);
        }
    }
}
