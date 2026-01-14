using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.SearchProducts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator(IOptions<SearchSettings> searchSettings)
    {
        var settings = searchSettings.Value;

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithMessage("Arama terimi en fazla 200 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Brand)
            .MaximumLength(100)
            .WithMessage("Marka adı en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Brand));

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum fiyat 0 veya daha büyük olmalıdır.")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maksimum fiyat 0 veya daha büyük olmalıdır.")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5)
            .WithMessage("Minimum puan 0 ile 5 arasında olmalıdır.")
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.SortBy)
            .MaximumLength(50)
            .WithMessage("Sıralama kriteri en fazla 50 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 1'den büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");
    }
}
