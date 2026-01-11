using FluentValidation;

namespace Merge.Application.Product.Queries.GetComparisonMatrix;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GetComparisonMatrixQueryValidator : AbstractValidator<GetComparisonMatrixQuery>
{
    public GetComparisonMatrixQueryValidator()
    {
        RuleFor(x => x.ComparisonId)
            .NotEmpty().WithMessage("Comparison ID is required");
    }
}
