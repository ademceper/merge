using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetTopReviewers;

public class GetTopReviewersQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetTopReviewersQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetTopReviewersQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Limit {maxPageSize}'den büyük olamaz");
    }
}

