using MediatR;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;

namespace Merge.Application.ML.Queries.OptimizePricesForCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record OptimizePricesForCategoryQuery(
    Guid CategoryId,
    PriceOptimizationRequestDto? Request = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<PriceOptimizationDto>>;
