using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetComparisonMatrix;

public class GetComparisonMatrixQueryValidator : AbstractValidator<GetComparisonMatrixQuery>
{
    public GetComparisonMatrixQueryValidator()
    {
        RuleFor(x => x.ComparisonId)
            .NotEmpty().WithMessage("Comparison ID is required");
    }
}
