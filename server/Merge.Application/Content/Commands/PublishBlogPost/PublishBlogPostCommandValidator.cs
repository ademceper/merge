using FluentValidation;

namespace Merge.Application.Content.Commands.PublishBlogPost;

public class PublishBlogPostCommandValidator : AbstractValidator<PublishBlogPostCommand>
{
    public PublishBlogPostCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Blog post ID'si zorunludur.");
    }
}

