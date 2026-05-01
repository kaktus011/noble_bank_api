using MediatR;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Cards.Commands.RequestCard
{
    /// <summary>
    /// Internal pipeline command used by the handler.
    /// Not exposed by API contracts/Swagger.
    /// </summary>
    public record RequestCardCommandInternal(
        string UserId,
        CardEnum.Type Type,
        CardEnum.Brand Brand,
        decimal? CreditLimit) : IRequest<CardDto>;
}