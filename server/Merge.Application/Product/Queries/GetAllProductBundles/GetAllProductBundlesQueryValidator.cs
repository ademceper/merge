using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllProductBundles;

public class GetAllProductBundlesQueryValidator : AbstractValidator<GetAllProductBundlesQuery>
{
    public GetAllProductBundlesQueryValidator()
    {
        // ActiveOnly is a boolean, no validation needed
    }
}
