using FluentValidation;

namespace Merge.Application.Content.Commands.PublishLandingPage;

public class PublishLandingPageCommandValidator : AbstractValidator<PublishLandingPageCommand>
{
    public PublishLandingPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

