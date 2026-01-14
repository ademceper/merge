using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Content.Commands.UpdateBanner;

public class UpdateBannerCommandValidator : AbstractValidator<UpdateBannerCommand>
{
    public UpdateBannerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Banner ID'si zorunludur.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Banner basligi zorunludur.")
            .MaximumLength(200)
            .WithMessage("Banner basligi en fazla 200 karakter olabilir.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .WithMessage("Banner gorsel URL'i zorunludur.")
            .MaximumLength(500)
            .WithMessage("Gorsel URL'i en fazla 500 karakter olabilir.");

        RuleFor(x => x.Position)
            .NotEmpty()
            .WithMessage("Banner pozisyonu zorunludur.")
            .MaximumLength(50)
            .WithMessage("Pozisyon en fazla 50 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Aciklama en fazla 1000 karakter olabilir.");

        RuleFor(x => x.LinkUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.LinkUrl))
            .WithMessage("Link URL'i en fazla 500 karakter olabilir.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Siralama 0 veya daha buyuk olmalidir.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Bitis tarihi baslangic tarihinden sonra olmalidir.");
    }
}
