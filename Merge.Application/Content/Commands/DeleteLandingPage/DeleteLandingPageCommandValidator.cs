using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteLandingPage;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class DeleteLandingPageCommandValidator : AbstractValidator<DeleteLandingPageCommand>
{
    public DeleteLandingPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

