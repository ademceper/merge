using FluentValidation;

namespace Merge.Application.Product.Queries.SearchProducts;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Arama terimi bos olamaz.")
            .MinimumLength(2)
            .WithMessage("Arama terimi en az 2 karakter olmalidir.")
            .MaximumLength(100)
            .WithMessage("Arama terimi en fazla 100 karakter olabilir.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarasi en az 1 olmalidir.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu 1 ile 100 arasinda olmalidir.");
    }
}
