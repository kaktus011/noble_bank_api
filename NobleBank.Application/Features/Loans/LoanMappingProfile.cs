using AutoMapper;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Loans;

public class LoanMappingProfile : Profile
{
    public LoanMappingProfile()
    {
        CreateMap<Loan, LoanDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.ProgressPercentage,
                o => o.MapFrom(s => s.Amount == 0 ? 0 :
                    Math.Round((s.Amount - s.RemainingAmount) / s.Amount * 100, 2)))
            .ForMember(d => d.RejectionReason, o => o.MapFrom(s => s.RejectionReason))
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId));
    }
}