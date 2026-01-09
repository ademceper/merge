using FluentValidation;

namespace Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateOrUpdateSEOSettingsCommandValidator : AbstractValidator<CreateOrUpdateSEOSettingsCommand>
{
    public CreateOrUpdateSEOSettingsCommandValidator()
    {
        RuleFor(x => x.PageType)
            .NotEmpty()
            .WithMessage("Sayfa tipi zorunludur.")
            .MaximumLength(50)
            .WithMessage("Sayfa tipi en fazla 50 karakter olabilir.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.MetaTitle))
            .WithMessage("Meta başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.MetaDescription))
            .WithMessage("Meta açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.MetaKeywords))
            .WithMessage("Meta anahtar kelimeler en fazla 200 karakter olabilir.");

        RuleFor(x => x.CanonicalUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.CanonicalUrl))
            .WithMessage("Canonical URL en fazla 500 karakter olabilir.");

        RuleFor(x => x.OgTitle)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.OgTitle))
            .WithMessage("Open Graph başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.OgDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.OgDescription))
            .WithMessage("Open Graph açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.OgImageUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.OgImageUrl))
            .WithMessage("Open Graph görsel URL'i en fazla 500 karakter olabilir.");

        RuleFor(x => x.TwitterCard)
            .Must(card => string.IsNullOrEmpty(card) || card == "summary" || card == "summary_large_image")
            .When(x => !string.IsNullOrEmpty(x.TwitterCard))
            .WithMessage("Twitter card 'summary' veya 'summary_large_image' olmalıdır.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 1)
            .WithMessage("Priority 0.0 ile 1.0 arasında olmalıdır.");

        RuleFor(x => x.ChangeFrequency)
            .Must(freq => string.IsNullOrEmpty(freq) || 
                new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" }
                    .Contains(freq.ToLowerInvariant()))
            .When(x => !string.IsNullOrEmpty(x.ChangeFrequency))
            .WithMessage("Geçersiz change frequency. Geçerli değerler: always, hourly, daily, weekly, monthly, yearly, never");
    }
}

