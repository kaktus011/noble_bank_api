using FluentValidation;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Admin.Commands.RejectCard
{
    public class RejectCardCommandValidator : AbstractValidator<RejectCardCommand>
    {
        public RejectCardCommandValidator()
        {
            RuleFor(x => x.CardId)
                .NotEmpty().WithMessage(Constants.Requirements.CardIdRequired);

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage(Constants.Requirements.RejectionReasonRequired)
                .MaximumLength(500).WithMessage(Constants.Requirements.RejectionReasonMaxLength);
        }
    }
}
