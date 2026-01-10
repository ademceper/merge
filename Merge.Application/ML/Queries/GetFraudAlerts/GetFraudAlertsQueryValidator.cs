using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.ML.Queries.GetFraudAlerts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class GetFraudAlertsQueryValidator : AbstractValidator<GetFraudAlertsQuery>
{
    public GetFraudAlertsQueryValidator(
        IOptions<PaginationSettings> paginationSettings,
        IOptions<MLSettings> mlSettings)
    {
        var paginationConfig = paginationSettings.Value;
        var mlConfig = mlSettings.Value;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(paginationConfig.MaxPageSize).WithMessage($"Page size cannot exceed {paginationConfig.MaxPageSize}.");

        RuleFor(x => x.MinRiskScore)
            .InclusiveBetween(0, mlConfig.MaxRiskScore).WithMessage($"Minimum risk score must be between 0 and {mlConfig.MaxRiskScore}.")
            .When(x => x.MinRiskScore.HasValue);
    }
}
