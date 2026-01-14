using FluentValidation;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Product.Queries.GetUserComparison;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GetUserComparisonQueryValidator : AbstractValidator<GetUserComparisonQuery>
{
    public GetUserComparisonQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
