using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.ML.Queries.ForecastDemandForCategory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class ForecastDemandForCategoryQueryValidator : AbstractValidator<ForecastDemandForCategoryQuery>
{
    public ForecastDemandForCategoryQueryValidator(
        IOptions<MLSettings> mlSettings,
        IOptions<PaginationSettings> paginationSettings)
    {
        var mlConfig = mlSettings.Value;
        var paginationConfig = paginationSettings.Value;

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.ForecastDays)
            .GreaterThan(0).WithMessage("Forecast days must be greater than 0.")
            .LessThanOrEqualTo(mlConfig.MaxForecastDays).WithMessage($"Forecast days cannot exceed {mlConfig.MaxForecastDays}.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(paginationConfig.MaxPageSize).WithMessage($"Page size cannot exceed {paginationConfig.MaxPageSize}.");
    }
}
