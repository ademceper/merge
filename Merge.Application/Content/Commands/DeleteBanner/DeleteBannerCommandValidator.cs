using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteBanner;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteBannerCommandValidator : AbstractValidator<DeleteBannerCommand>
{
    public DeleteBannerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Banner ID'si zorunludur.");
    }
}

