using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetAdminTopProducts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetAdminTopProductsQueryValidator : AbstractValidator<GetAdminTopProductsQuery>
{
    public GetAdminTopProductsQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var maxPageSize = paginationSettings.Value.MaxPageSize;

        RuleFor(x => x.Count)
            .GreaterThan(0).WithMessage("Count 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Count {maxPageSize}'den büyük olamaz");
    }
}

