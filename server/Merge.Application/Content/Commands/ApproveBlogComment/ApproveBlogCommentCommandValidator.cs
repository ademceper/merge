using FluentValidation;

namespace Merge.Application.Content.Commands.ApproveBlogComment;

public class ApproveBlogCommentCommandValidator : AbstractValidator<ApproveBlogCommentCommand>
{
    public ApproveBlogCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Yorum ID'si zorunludur.");
    }
}

