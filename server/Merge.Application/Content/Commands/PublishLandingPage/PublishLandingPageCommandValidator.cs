using FluentValidation;

namespace Merge.Application.Content.Commands.PublishLandingPage;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class PublishLandingPageCommandValidator : AbstractValidator<PublishLandingPageCommand>
{
    public PublishLandingPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

