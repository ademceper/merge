using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetPendingReturns;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetPendingReturnsQueryValidator : AbstractValidator<GetPendingReturnsQuery>
{
    public GetPendingReturnsQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var maxPageSize = paginationSettings.Value.MaxPageSize;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(0).WithMessage("Sayfa boyutu 0 veya daha büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Sayfa boyutu {maxPageSize}'den büyük olamaz");
    }
}

