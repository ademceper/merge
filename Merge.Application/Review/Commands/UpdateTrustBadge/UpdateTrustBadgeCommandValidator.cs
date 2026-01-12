using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.UpdateTrustBadge;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateTrustBadgeCommandValidator : AbstractValidator<UpdateTrustBadgeCommand>
{
    public UpdateTrustBadgeCommandValidator()
    {
        RuleFor(x => x.BadgeId)
            .NotEmpty()
            .WithMessage("Badge ID'si zorunludur.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Badge adı en fazla 100 karakter olabilir.")
            .MinimumLength(2)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Badge adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Badge açıklaması en fazla 500 karakter olabilir.");

        RuleFor(x => x.IconUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.IconUrl))
            .WithMessage("İkon URL'si en fazla 500 karakter olabilir.")
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.IconUrl))
            .WithMessage("Geçerli bir URL giriniz.");

        RuleFor(x => x.BadgeType)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.BadgeType))
            .WithMessage("Badge tipi en fazla 50 karakter olabilir.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DisplayOrder.HasValue)
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
