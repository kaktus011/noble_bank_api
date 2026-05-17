using MediatR;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;

namespace NobleBank.Application.Features.Admin.Queries.GetPendingCards
{
    public record GetPendingCardsQuery : IRequest<List<CardDto>>;
}
