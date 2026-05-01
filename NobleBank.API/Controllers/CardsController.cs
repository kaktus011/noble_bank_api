using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobleBank.Application.Features.Cards.Commands.RequestCard;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Application.Features.Cards.Queries.GetCardById;

namespace NobleBank.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : BaseController
    {
        private readonly IMediator _mediator;

        public CardsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<CardDto>>> GetAll()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return Unauthorized();
            }

            List<CardDto> cards = await _mediator.Send(new GetAllCardsQuery(UserId));

            return Ok(cards);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CardDto>> GetById(Guid id)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return Unauthorized();
            }

            CardDto? card = await _mediator.Send(new GetCardByIdQuery(id, UserId));

            if (card is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Card not found");
            }

            return Ok(card);
        }

        [HttpPost("request")]
        public async Task<ActionResult<CardDto>> RequestCard([FromBody] RequestCardCommand command)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return Unauthorized();
            }

            RequestCardCommand commandWithUserId = command with { UserId = UserId };
            CardDto card = await _mediator.Send(commandWithUserId);

            return CreatedAtAction(nameof(GetById), new { id = card.Id }, card);
        }
    }
}
