using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobleBank.Application.Features.Admin.Commands.ApproveCard;
using NobleBank.Application.Features.Admin.Commands.ApproveLoan;
using NobleBank.Application.Features.Admin.Commands.RejectCard;
using NobleBank.Application.Features.Admin.Commands.RejectLoan;
using NobleBank.Application.Features.Admin.Queries.GetPendingCards;
using NobleBank.Application.Features.Admin.Queries.GetPendingLoans;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Common;

namespace NobleBank.API.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : BaseController
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ===== CARDS =====

        [HttpGet("cards/pending")]
        public async Task<ActionResult<List<CardDto>>> GetPendingCards()
        {
            var cards = await _mediator.Send(new GetPendingCardsQuery());
            return Ok(cards);
        }

        [HttpPost("cards/{id:guid}/approve")]
        public async Task<IActionResult> ApproveCard(Guid id)
        {
            await _mediator.Send(new ApproveCardCommand(UserId, id));
            return NoContent();
        }

        [HttpPost("cards/{id:guid}/reject")]
        public async Task<IActionResult> RejectCard(Guid id, [FromBody] RejectRequest request)
        {
            await _mediator.Send(new RejectCardCommand(UserId, id, request.Reason));
            return NoContent();
        }

        // ===== LOANS =====

        [HttpGet("loans/pending")]
        public async Task<ActionResult<List<LoanDto>>> GetPendingLoans()
        {
            var loans = await _mediator.Send(new GetPendingLoansQuery());
            return Ok(loans);
        }

        [HttpPost("loans/{id:guid}/approve")]
        public async Task<IActionResult> ApproveLoan(Guid id)
        {
            await _mediator.Send(new ApproveLoanCommand(UserId, id));
            return NoContent();
        }

        [HttpPost("loans/{id:guid}/reject")]
        public async Task<IActionResult> RejectLoan(Guid id, [FromBody] RejectRequest request)
        {
            await _mediator.Send(new RejectLoanCommand(UserId, id, request.Reason));
            return NoContent();
        }
    }

    public record RejectRequest(string Reason);
}
