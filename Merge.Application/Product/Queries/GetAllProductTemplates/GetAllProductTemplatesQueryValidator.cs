using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllProductTemplates;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GetAllProductTemplatesQueryValidator : AbstractValidator<GetAllProductTemplatesQuery>
{
    public GetAllProductTemplatesQueryValidator()
    {
        // CategoryId and IsActive are optional, no validation needed
    }
}
