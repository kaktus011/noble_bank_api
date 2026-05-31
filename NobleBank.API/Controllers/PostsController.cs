using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobleBank.Application.Features.Posts.Commands.CreatePost;
using NobleBank.Application.Features.Posts.Commands.DeletePost;
using NobleBank.Application.Features.Posts.Queries.GetAllPosts;
using NobleBank.Application.Features.Posts.Queries.GetPostById;
using NobleBank.Domain.Common;

namespace NobleBank.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : BaseController
    {
        private readonly IMediator _mediator;

        public PostsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<PostDto>>> GetAll()
        {
            List<PostDto> posts = await _mediator.Send(new GetAllPostsQuery());

            return Ok(posts);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PostDto>> GetById(Guid id)
        {
            PostDto? post = await _mediator.Send(new GetPostByIdQuery(id));

            if (post is null)
            {
                return NotFound(new { error = Constants.Exceptions.PostNotFound });
            }

            return Ok(post);
        }

        [Authorize(Roles = Roles.Administrator)]
        [HttpPost]
        public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostCommand command)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return Unauthorized();
            }

            CreatePostCommand commandWithUserId = command with { UserId = UserId };

            PostDto post = await _mediator.Send(commandWithUserId);

            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        [Authorize(Roles = Roles.Administrator)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return Unauthorized();
            }

            await _mediator.Send(new DeletePostCommand(UserId, id));

            return NoContent();
        }
    }
}
