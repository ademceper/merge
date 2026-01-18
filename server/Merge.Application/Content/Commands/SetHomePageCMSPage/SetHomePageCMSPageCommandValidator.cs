using FluentValidation;

namespace Merge.Application.Content.Commands.SetHomePageCMSPage;

public class SetHomePageCMSPageCommandValidator : AbstractValidator<SetHomePageCMSPageCommand>
{
    public SetHomePageCMSPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("CMS sayfası ID'si zorunludur.");
    }
}

