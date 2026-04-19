using FluentValidation;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Cards.Commands.RequestCard
{
    public class RequestCardCommandValidator : AbstractValidator<RequestCardCommand>
    {
        public RequestCardCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid card type.");

            RuleFor(x => x.Brand)
                .IsInEnum().WithMessage("Invalid card brand.");

            RuleFor(x => x.CreditLimit)
                .GreaterThan(0)
                .When(x => x.Type == CardEnum.Type.Credit)
                .WithMessage("Credit cards must have a credit limit greater than 0.");

            RuleFor(x => x.CreditLimit)
                .Null()
                .When(x => x.Type == CardEnum.Type.Debit || x.Type == CardEnum.Type.Virtual)
                .WithMessage("Debit and Virtual cards cannot have a credit limit.");
        }
    }
}