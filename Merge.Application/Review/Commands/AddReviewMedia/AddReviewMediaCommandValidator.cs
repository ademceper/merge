using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.AddReviewMedia;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class AddReviewMediaCommandValidator : AbstractValidator<AddReviewMediaCommand>
{
    public AddReviewMediaCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Değerlendirme ID'si zorunludur.");

        RuleFor(x => x.MediaType)
            .NotEmpty()
            .WithMessage("Medya tipi zorunludur.")
            .Must(BeAValidMediaType)
            .WithMessage("Geçerli bir medya tipi giriniz (Photo, Video).");

        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("URL zorunludur.")
            .MaximumLength(500)
            .WithMessage("URL en fazla 500 karakter olabilir.")
            .Must(BeAValidUrl)
            .WithMessage("Geçerli bir URL giriniz.");

        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ThumbnailUrl))
            .WithMessage("Thumbnail URL en fazla 500 karakter olabilir.")
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.ThumbnailUrl))
            .WithMessage("Geçerli bir thumbnail URL giriniz.");

        RuleFor(x => x.FileSize)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Dosya boyutu negatif olamaz.");

        RuleFor(x => x.Width)
            .GreaterThan(0)
            .When(x => x.Width.HasValue)
            .WithMessage("Genişlik 0'dan büyük olmalıdır.");

        RuleFor(x => x.Height)
            .GreaterThan(0)
            .When(x => x.Height.HasValue)
            .WithMessage("Yükseklik 0'dan büyük olmalıdır.");

        RuleFor(x => x.Duration)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Duration.HasValue)
            .WithMessage("Süre negatif olamaz.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Görüntüleme sırası negatif olamaz.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private bool BeAValidMediaType(string mediaType)
    {
        return Enum.TryParse<Merge.Domain.Enums.ReviewMediaType>(mediaType, true, out _);
    }
}
