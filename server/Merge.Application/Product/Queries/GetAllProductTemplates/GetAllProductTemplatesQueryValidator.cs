using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllProductTemplates;

public class GetAllProductTemplatesQueryValidator : AbstractValidator<GetAllProductTemplatesQuery>
{
    public GetAllProductTemplatesQueryValidator()
    {
        // CategoryId and IsActive are optional, no validation needed
    }
}
