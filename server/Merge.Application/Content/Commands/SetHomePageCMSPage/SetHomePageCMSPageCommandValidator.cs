using FluentValidation;

namespace Merge.Application.Content.Commands.SetHomePageCMSPage;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SetHomePageCMSPageCommandValidator : AbstractValidator<SetHomePageCMSPageCommand>
{
    public SetHomePageCMSPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("CMS sayfası ID'si zorunludur.");
    }
}

