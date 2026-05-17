using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Posts.Commands.DeletePost
{
    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeletePostCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new UnauthorizedAccessException(Constants.Requirements.UserIdRequired);
            }

            Post? post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == request.PostId && p.UserId == request.UserId,
                    cancellationToken);

            if (post is null)
            {
                throw new Exception(Constants.Exceptions.PostNotFound);
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
