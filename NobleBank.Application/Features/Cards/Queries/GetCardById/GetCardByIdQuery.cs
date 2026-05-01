using MediatR;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;

namespace NobleBank.Application.Features.Cards.Queries.GetCardById
{
    public record GetCardByIdQuery(Guid CardId, string UserId) : IRequest<CardDto?>;
}
