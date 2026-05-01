using AutoMapper;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Domain.Entities;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Cards
{
    public class CardMappingProfile : Profile
    {
        public CardMappingProfile()
        {
            CreateMap<Card, CardDto>()
                .ForMember(d => d.Brand, o => o.MapFrom(s => s.Brand.ToString()))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.CardType.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.ExpiryDate, o => o.MapFrom(s => s.ExpiryDate))
                .ForMember(d => d.IsCredit, o => o.MapFrom(s => s.CardType == CardEnum.Type.Credit))
                .ForMember(d => d.MaskedNumber, o => o.MapFrom(s => s.Last4Digits))
                .ForMember(d => d.IsExpired, o => o.MapFrom(s => s.ExpiryDate < DateTime.UtcNow));
        }
    }
}
