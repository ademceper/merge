using FluentValidation;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Product.Queries.GetUserComparison;

public class GetUserComparisonQueryValidator : AbstractValidator<GetUserComparisonQuery>
{
    public GetUserComparisonQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
