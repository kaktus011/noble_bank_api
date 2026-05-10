using NobleBank.Application.Features.Transactions.Commands.CreateTransaction;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Application.Features.Transactions.Queries.GetTransactionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NobleBank.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : BaseController
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> GetAll(
        [FromQuery] Guid? cardId = null,
        [FromQuery] int? limit = 50)
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            return Unauthorized();
        }

        List<TransactionDto> transactions = await _mediator.Send(
            new GetAllTransactionsQuery(UserId, cardId, limit));

        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id)
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            return Unauthorized();
        }

        TransactionDto? transaction = await _mediator.Send(
            new GetTransactionByIdQuery(id, UserId));

        if (transaction is null)
        {
            return NotFound(new { error = "Transaction not found" });
        }

        return Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create(
        [FromBody] CreateTransactionCommand command)
    {
        CreateTransactionCommand commandWithUserId = command with { UserId = UserId };

        TransactionDto transaction = await _mediator.Send(commandWithUserId);

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }
}