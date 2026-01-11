using FluentValidation;

namespace Merge.Application.Product.Queries.GetAllProductBundles;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GetAllProductBundlesQueryValidator : AbstractValidator<GetAllProductBundlesQuery>
{
    public GetAllProductBundlesQueryValidator()
    {
        // ActiveOnly is a boolean, no validation needed
    }
}
