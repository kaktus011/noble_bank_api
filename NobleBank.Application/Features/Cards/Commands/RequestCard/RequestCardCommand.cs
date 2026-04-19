using MediatR;
using NobleBank.Domain.Common;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;

namespace NobleBank.Application.Features.Cards.Commands.RequestCard
{
    public record RequestCardCommand(
        string UserId,
        CardEnum.Type Type,
        CardEnum.Brand Brand,
        decimal? CreditLimit) : IRequest<CardDto>;
}