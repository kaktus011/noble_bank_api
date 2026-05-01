using MediatR;

namespace NobleBank.Application.Features.Cards.Queries.GetAllCards
{
    public record GetAllCardsQuery(string UserId) : IRequest<List<CardDto>>;
}
