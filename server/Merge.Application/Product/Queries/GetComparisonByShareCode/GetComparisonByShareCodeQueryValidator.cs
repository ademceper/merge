using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetComparisonByShareCode;

public class GetComparisonByShareCodeQueryValidator : AbstractValidator<GetComparisonByShareCodeQuery>
{
    public GetComparisonByShareCodeQueryValidator()
    {
        RuleFor(x => x.ShareCode)
            .NotEmpty().WithMessage("Share code is required")
            .MinimumLength(6).WithMessage("Share code must be at least 6 characters")
            .MaximumLength(50).WithMessage("Share code must not exceed 50 characters");
    }
}
