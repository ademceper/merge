using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetQAStats;

public class GetQAStatsQueryValidator : AbstractValidator<GetQAStatsQuery>
{
    public GetQAStatsQueryValidator()
    {
        // ProductId is optional, no validation needed
    }
}
