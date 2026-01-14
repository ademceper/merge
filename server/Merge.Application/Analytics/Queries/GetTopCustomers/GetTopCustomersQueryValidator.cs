using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetTopCustomers;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetTopCustomersQueryValidator : AbstractValidator<GetTopCustomersQuery>
{
    public GetTopCustomersQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var maxPageSize = paginationSettings.Value.MaxPageSize;

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Limit {maxPageSize}'den büyük olamaz");
    }
}

