using FluentValidation;

namespace Merge.Application.Product.Queries.GetAllProducts;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAllProductsQueryValidator : AbstractValidator<GetAllProductsQuery>
{
    public GetAllProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarasi en az 1 olmalidir.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu 1 ile 100 arasinda olmalidir.");
    }
}
