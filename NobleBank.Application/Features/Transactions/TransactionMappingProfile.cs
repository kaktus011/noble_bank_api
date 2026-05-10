using AutoMapper;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Transactions;

public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.CardLast4, o => o.MapFrom(s => s.Card.Last4Digits));
    }
}