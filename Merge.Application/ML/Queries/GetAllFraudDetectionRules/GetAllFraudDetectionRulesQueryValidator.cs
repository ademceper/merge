using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.ML.Queries.GetAllFraudDetectionRules;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class GetAllFraudDetectionRulesQueryValidator : AbstractValidator<GetAllFraudDetectionRulesQuery>
{
    public GetAllFraudDetectionRulesQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var paginationConfig = paginationSettings.Value;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(paginationConfig.MaxPageSize).WithMessage($"Page size cannot exceed {paginationConfig.MaxPageSize}.");
    }
}
