using NobleBank.Application.Features.Loans.Commands.RequestLoan;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Application.Features.Loans.Queries.GetLoanById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NobleBank.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoansController : BaseController
{
    private readonly IMediator _mediator;

    public LoansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<LoanDto>>> GetAll()
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            return Unauthorized();
        }

        List<LoanDto> loans = await _mediator.Send(new GetAllLoansQuery(UserId));

        return Ok(loans);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LoanDto>> GetById(Guid id)
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            return Unauthorized();
        }

        LoanDto? loan = await _mediator.Send(new GetLoanByIdQuery(id, UserId));

        if (loan is null)
        {
            return NotFound(new { error = "Loan not found" });
        }

        return Ok(loan);
    }

    [HttpPost("request")]
    public async Task<ActionResult<LoanDto>> RequestLoan([FromBody] RequestLoanCommand command)
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            return Unauthorized();
        }

        RequestLoanCommand commandWithUserId = command with { UserId = UserId };

        LoanDto loan = await _mediator.Send(commandWithUserId);

        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }
}