using FluentValidation;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Constants.Requirements.EmailRequired)
                .EmailAddress().WithMessage(Constants.ErrrorMessages.IvalidEmailFormat);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Constants.Requirements.PasswordRequired);
        }
    }
}
