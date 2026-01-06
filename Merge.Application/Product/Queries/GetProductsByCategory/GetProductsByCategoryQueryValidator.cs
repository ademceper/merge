using FluentValidation;

namespace Merge.Application.Product.Queries.GetProductsByCategory;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetProductsByCategoryQueryValidator : AbstractValidator<GetProductsByCategoryQuery>
{
    public GetProductsByCategoryQueryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarasi en az 1 olmalidir.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu 1 ile 100 arasinda olmalidir.");
    }
}
