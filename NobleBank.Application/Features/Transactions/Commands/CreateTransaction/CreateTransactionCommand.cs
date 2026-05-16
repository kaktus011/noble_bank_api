using MediatR;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Domain.Common;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Transactions.Commands.CreateTransaction
{
    public record CreateTransactionCommand(
        [property: JsonIgnore]
    string? UserId,

        Guid CardId,
        decimal Amount,
        string Description,
        TransactionsEnum.Type Type) : IRequest<TransactionDto>;
}