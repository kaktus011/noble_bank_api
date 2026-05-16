using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using MediatR;

namespace NobleBank.Application.Features.Transactions.Queries.GetTransactionById
{
    public record GetTransactionByIdQuery(Guid TransactionId, string UserId)
        : IRequest<TransactionDto?>;
}