using FluentValidation;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Admin.Commands.RejectLoan
{
    public class RejectLoanCommandValidator : AbstractValidator<RejectLoanCommand>
    {
        public RejectLoanCommandValidator()
        {
            RuleFor(x => x.LoanId)
                .NotEmpty().WithMessage(Constants.Requirements.LoanIdRequired);

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage(Constants.Requirements.RejectionReasonRequired)
                .MaximumLength(500).WithMessage(Constants.Requirements.RejectionReasonMaxLength);
        }
    }
}
