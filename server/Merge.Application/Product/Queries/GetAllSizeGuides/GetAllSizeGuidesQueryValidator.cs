using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllSizeGuides;

public class GetAllSizeGuidesQueryValidator : AbstractValidator<GetAllSizeGuidesQuery>
{
    public GetAllSizeGuidesQueryValidator()
    {
        // Empty query, no validation needed
    }
}
