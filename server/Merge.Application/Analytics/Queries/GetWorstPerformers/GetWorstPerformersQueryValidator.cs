using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetWorstPerformers;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetWorstPerformersQueryValidator : AbstractValidator<GetWorstPerformersQuery>
{
    public GetWorstPerformersQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var maxPageSize = paginationSettings.Value.MaxPageSize;

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Limit {maxPageSize}'den büyük olamaz");
    }
}

