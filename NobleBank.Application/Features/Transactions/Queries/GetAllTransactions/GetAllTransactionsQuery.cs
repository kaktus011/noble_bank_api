using MediatR;

namespace NobleBank.Application.Features.Transactions.Queries.GetAllTransactions
{
    public record GetAllTransactionsQuery(
        string UserId,
        Guid? CardId = null,
        int? Limit = 50) : IRequest<List<TransactionDto>>;
}