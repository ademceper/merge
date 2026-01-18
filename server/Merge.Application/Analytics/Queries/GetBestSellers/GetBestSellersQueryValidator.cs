using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetBestSellers;

public class GetBestSellersQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetBestSellersQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetBestSellersQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Limit {maxPageSize}'den büyük olamaz");
    }
}

