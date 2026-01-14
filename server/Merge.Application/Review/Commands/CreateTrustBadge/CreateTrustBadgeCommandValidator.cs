using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.CreateTrustBadge;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateTrustBadgeCommandValidator : AbstractValidator<CreateTrustBadgeCommand>
{
    public CreateTrustBadgeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Badge adı zorunludur.")
            .MaximumLength(100)
            .WithMessage("Badge adı en fazla 100 karakter olabilir.")
            .MinimumLength(2)
            .WithMessage("Badge adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Badge açıklaması zorunludur.")
            .MaximumLength(500)
            .WithMessage("Badge açıklaması en fazla 500 karakter olabilir.");

        RuleFor(x => x.IconUrl)
            .NotEmpty()
            .WithMessage("İkon URL'si zorunludur.")
            .MaximumLength(500)
            .WithMessage("İkon URL'si en fazla 500 karakter olabilir.")
            .Must(BeAValidUrl)
            .WithMessage("Geçerli bir URL giriniz.");

        RuleFor(x => x.BadgeType)
            .NotEmpty()
            .WithMessage("Badge tipi zorunludur.")
            .MaximumLength(50)
            .WithMessage("Badge tipi en fazla 50 karakter olabilir.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Görüntüleme sırası negatif olamaz.");

        RuleFor(x => x.Color)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Color))
            .WithMessage("Renk en fazla 20 karakter olabilir.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
