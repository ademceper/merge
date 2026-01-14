using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllSizeGuides;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GetAllSizeGuidesQueryValidator : AbstractValidator<GetAllSizeGuidesQuery>
{
    public GetAllSizeGuidesQueryValidator()
    {
        // Empty query, no validation needed
    }
}
