using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetWorstPerformers;

public class GetWorstPerformersQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetWorstPerformersQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetWorstPerformersQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Limit {maxPageSize}'den büyük olamaz");
    }
}

