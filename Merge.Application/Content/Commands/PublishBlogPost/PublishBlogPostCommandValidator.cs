using FluentValidation;

namespace Merge.Application.Content.Commands.PublishBlogPost;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class PublishBlogPostCommandValidator : AbstractValidator<PublishBlogPostCommand>
{
    public PublishBlogPostCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Blog post ID'si zorunludur.");
    }
}

