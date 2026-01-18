using FluentValidation;

namespace Merge.Application.Content.Commands.PublishCMSPage;

public class PublishCMSPageCommandValidator : AbstractValidator<PublishCMSPageCommand>
{
    public PublishCMSPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("CMS sayfası ID'si zorunludur.");
    }
}

