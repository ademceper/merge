using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteCMSPage;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteCMSPageCommandValidator : AbstractValidator<DeleteCMSPageCommand>
{
    public DeleteCMSPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("CMS sayfası ID'si zorunludur.");
    }
}

