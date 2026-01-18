using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteBlogComment;

public class DeleteBlogCommentCommandValidator : AbstractValidator<DeleteBlogCommentCommand>
{
    public DeleteBlogCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Yorum ID'si zorunludur.");
    }
}

