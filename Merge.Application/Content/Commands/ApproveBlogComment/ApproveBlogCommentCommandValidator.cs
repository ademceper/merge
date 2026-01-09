using FluentValidation;

namespace Merge.Application.Content.Commands.ApproveBlogComment;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ApproveBlogCommentCommandValidator : AbstractValidator<ApproveBlogCommentCommand>
{
    public ApproveBlogCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Yorum ID'si zorunludur.");
    }
}

