using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.ML.Queries.OptimizePricesForCategory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class OptimizePricesForCategoryQueryValidator : AbstractValidator<OptimizePricesForCategoryQuery>
{
    public OptimizePricesForCategoryQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var paginationConfig = paginationSettings.Value;

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(paginationConfig.MaxPageSize).WithMessage($"Page size cannot exceed {paginationConfig.MaxPageSize}.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request!.MinPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum price must be greater than or equal to 0.")
                .When(x => x.Request!.MinPrice.HasValue);

            RuleFor(x => x.Request!.MaxPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Maximum price must be greater than or equal to 0.")
                .When(x => x.Request!.MaxPrice.HasValue);

            RuleFor(x => x.Request!.Strategy)
                .MaximumLength(100).WithMessage("Strategy cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Request!.Strategy));
        });
    }
}
