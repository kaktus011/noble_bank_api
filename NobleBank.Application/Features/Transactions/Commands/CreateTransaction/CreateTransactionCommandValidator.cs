using FluentValidation;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Transactions.Commands.CreateTransaction
{
    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        public CreateTransactionCommandValidator()
        {
            RuleFor(x => x.CardId)
                .NotEmpty().WithMessage(Constants.Requirements.CardIdRequired);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage(Constants.Requirements.TransactionAmountPositive);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(Constants.Requirements.TransactionDescriptionRequired)
                .MaximumLength(250).WithMessage(Constants.Requirements.TransactionDescriptionMaxLength);
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage(Constants.Exceptions.IvalidTransactionType);
        }
    }
}