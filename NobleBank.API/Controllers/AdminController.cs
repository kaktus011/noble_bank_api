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
using NobleBank.Application.Features.Cards.Queries.GetCardById;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Application.Features.Loans.Queries.GetLoanById;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Application.Features.Transactions.Queries.GetTransactionById;
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

        [HttpGet("cards")]
        public async Task<ActionResult<List<CardDto>>> GetAllCards()
        {
            List<CardDto> cards = await _mediator.Send(new GetAllCardsQuery(null));

            return Ok(cards);
        }

        [HttpGet("cards/{id:guid}")]
        public async Task<ActionResult<CardDto>> GetCardById(Guid id)
        {
            CardDto? card = await _mediator.Send(new GetCardByIdQuery(id, null));

            if (card is null)
            {
                return NotFound();
            }

            return Ok(card);
        }

        [HttpGet("cards/pending")]
        public async Task<ActionResult<List<CardDto>>> GetPendingCards()
        {
            List<CardDto> cards = await _mediator.Send(new GetPendingCardsQuery());

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

        [HttpGet("loans")]
        public async Task<ActionResult<List<LoanDto>>> GetAllLoans()
        {
            List<LoanDto> loans = await _mediator.Send(new GetAllLoansQuery(null));

            return Ok(loans);
        }

        [HttpGet("loans/{id:guid}")]
        public async Task<ActionResult<LoanDto>> GetLoanById(Guid id)
        {
            LoanDto? loan = await _mediator.Send(new GetLoanByIdQuery(id, null));

            if (loan is null)
            {
                return NotFound();
            }

            return Ok(loan);
        }

        [HttpGet("loans/pending")]
        public async Task<ActionResult<List<LoanDto>>> GetPendingLoans()
        {
            List<LoanDto> loans = await _mediator.Send(new GetPendingLoansQuery());

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

        // ===== TRANSACTIONS =====

        [HttpGet("transactions")]
        public async Task<ActionResult<List<TransactionDto>>> GetAllTransactions()
        {
            List<TransactionDto> transactions = await _mediator.Send(new GetAllTransactionsQuery(null, null, 50));

            return Ok(transactions);
        }

        [HttpGet("transactions/{id:guid}")]
        public async Task<ActionResult<TransactionDto>> GetTransactionById(Guid id)
        {
            TransactionDto? transaction = await _mediator.Send(new GetTransactionByIdQuery(id, null));

            if (transaction is null)
            {
                return NotFound();
            }

            return Ok(transaction);
        }
    }

    public record RejectRequest(string Reason);
}
