using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobleBank.Application.Features.Cards.Commands.RequestCard;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Application.Features.Cards.Queries.GetCardById;
using System.Security.Claims;

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
            var cards = await _mediator.Send(new GetAllCardsQuery(UserId));
            return Ok(cards);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CardDto>> GetById(Guid id)
        {
            var card = await _mediator.Send(new GetCardByIdQuery(id, UserId));

            if (card is null)
            {
                return NotFound(new { error = "Card not found" });
            }

            return Ok(card);
        }

        [HttpPost("request")]
        public new async Task<ActionResult<CardDto>> Request([FromBody] RequestCardCommand command)
        {
            // Override UserId от токена — не доверяваме на клиента
            var commandWithUserId = command with { UserId = UserId };

            var card = await _mediator.Send(commandWithUserId);

            return CreatedAtAction(nameof(GetById), new { id = card.Id }, card);
        }
    }
}
