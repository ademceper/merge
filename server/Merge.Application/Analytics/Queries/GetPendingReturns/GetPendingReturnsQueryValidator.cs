using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetPendingReturns;

public class GetPendingReturnsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetPendingReturnsQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetPendingReturnsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(0).WithMessage("Sayfa boyutu 0 veya daha büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Sayfa boyutu {maxPageSize}'den büyük olamaz");
    }
}

