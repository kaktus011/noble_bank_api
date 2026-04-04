using FluentValidation;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Constants.Requirements.EmailRequired)
                .EmailAddress().WithMessage(Constants.ErrorMessages.InvalidEmailFormat);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Constants.Requirements.PasswordRequired)
                .MinimumLength(8).WithMessage(Constants.Requirements.PasswordLength)
                .Matches("[A-Z]").WithMessage(Constants.Requirements.PasswordUppercase)
                .Matches("[0-9]").WithMessage(Constants.Requirements.PasswordNumber)
                .Matches("[^a-zA-Z0-9]").WithMessage(Constants.Requirements.PasswordSpecialChar);

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(Constants.Requirements.FirstNameRequired)
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(Constants.Requirements.LastNameRequired)
                .MaximumLength(50);
        }
    }
}