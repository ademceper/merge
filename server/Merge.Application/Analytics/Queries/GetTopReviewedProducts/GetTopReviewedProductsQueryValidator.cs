using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetTopReviewedProducts;

public class GetTopReviewedProductsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetTopReviewedProductsQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetTopReviewedProductsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Limit {maxPageSize}'den büyük olamaz");
    }
}

