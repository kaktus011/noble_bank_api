using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandler
    : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateTransactionCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TransactionDto> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            throw new UnauthorizedAccessException("User ID is required");
        }

        Card? card = await _context.Cards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && c.UserId == request.UserId,
                cancellationToken);

        if (card is null)
        {
            throw new NotFoundException("Card not found or does not belong to user");
        }

        // Apply transaction to card
        var result = request.Type switch
        {
            TransactionsEnum.Type.Income => card.Deposit(request.Amount, request.UserId),
            TransactionsEnum.Type.Expense => card.Withdraw(request.Amount, request.UserId),
            _ => throw new DomainException("Transfer type not yet implemented")
        };

        if (result.IsFail)
        {
            throw new DomainException(result.Error);
        }

        // Transaction record
        Transaction transaction = Transaction.Create(
            amount: request.Amount,
            description: request.Description,
            type: request.Type,
            cardId: request.CardId,
            performedBy: request.UserId
        );

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TransactionDto>(transaction);
    }
}