using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetTopCustomers;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetTopCustomersQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetTopCustomersQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetTopCustomersQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Limit {maxPageSize}'den büyük olamaz");
    }
}

