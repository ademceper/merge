using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.SearchProducts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var settings = paginationSettings.Value;

        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Arama terimi boş olamaz.")
            .MinimumLength(2)
            .WithMessage("Arama terimi en az 2 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Arama terimi en fazla 100 karakter olabilir.");

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
