namespace NobleBank.Application.Features.Loans.Queries.GetLoanOptions
{
    public record LoanEnumOption(int Value, string Name);

    public record LoanOptionsDto(IReadOnlyList<LoanEnumOption> Types);
}
