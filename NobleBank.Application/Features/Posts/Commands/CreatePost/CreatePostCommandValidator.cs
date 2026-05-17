using FluentValidation;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
    {
        public CreatePostCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(Constants.Requirements.PostTitleRequired)
                .MaximumLength(200).WithMessage(Constants.Requirements.PostTitleMaxLength);

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage(Constants.Requirements.PostBodyRequired)
                .MaximumLength(5000).WithMessage(Constants.Requirements.PostBodyMaxLength);
        }
    }
}
