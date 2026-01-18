using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.ML.Queries.GetAllFraudDetectionRules;

public class GetAllFraudDetectionRulesQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetAllFraudDetectionRulesQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetAllFraudDetectionRulesQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(config.MaxPageSize).WithMessage($"Page size cannot exceed {config.MaxPageSize}.");
    }
}
