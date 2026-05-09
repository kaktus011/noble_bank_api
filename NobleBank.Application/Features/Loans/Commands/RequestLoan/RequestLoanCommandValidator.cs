using FluentValidation;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Loans.Commands.RequestLoan;

public class RequestLoanCommandValidator : AbstractValidator<RequestLoanCommand>
{
    public RequestLoanCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(Constants.Requirements.LoanMinAmount)
            .LessThanOrEqualTo(100000).WithMessage(Constants.Requirements.LoanMaxAmount);

        RuleFor(x => x.TermMonths)
            .GreaterThan(0).WithMessage(Constants.Requirements.LoanMinTerm)
            .LessThanOrEqualTo(360).WithMessage(Constants.Requirements.LoanMaxTerm);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage(Constants.ErrorMessages.LoanTypeInvalid);
    }
}