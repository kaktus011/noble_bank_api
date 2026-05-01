using MediatR;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Domain.Common;
using System.Text.Json.Serialization;

namespace NobleBank.Application.Features.Cards.Commands.RequestCard
{
    public record RequestCardCommand(
        [property: JsonIgnore]
        string? UserId,

        CardEnum.Type Type,
        CardEnum.Brand Brand,
        decimal? CreditLimit) : IRequest<CardDto>;
}