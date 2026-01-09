using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteBlogPost;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteBlogPostCommandValidator : AbstractValidator<DeleteBlogPostCommand>
{
    public DeleteBlogPostCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Blog post ID'si zorunludur.");
    }
}

