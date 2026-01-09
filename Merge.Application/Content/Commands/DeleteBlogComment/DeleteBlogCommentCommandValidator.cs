using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteBlogComment;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteBlogCommentCommandValidator : AbstractValidator<DeleteBlogCommentCommand>
{
    public DeleteBlogCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Yorum ID'si zorunludur.");
    }
}

