using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductComparisonById;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GetProductComparisonByIdQueryValidator : AbstractValidator<GetProductComparisonByIdQuery>
{
    public GetProductComparisonByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comparison ID is required");
    }
}
