using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetQAStats;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GetQAStatsQueryValidator : AbstractValidator<GetQAStatsQuery>
{
    public GetQAStatsQueryValidator()
    {
        // ProductId is optional, no validation needed
    }
}
