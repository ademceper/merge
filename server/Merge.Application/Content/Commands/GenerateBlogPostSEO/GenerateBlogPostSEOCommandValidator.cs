using FluentValidation;

namespace Merge.Application.Content.Commands.GenerateBlogPostSEO;

public class GenerateBlogPostSEOCommandValidator : AbstractValidator<GenerateBlogPostSEOCommand>
{
    public GenerateBlogPostSEOCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Blog post ID'si zorunludur.");
    }
}

