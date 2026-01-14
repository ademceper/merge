using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetCategoryPerformance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCategoryPerformanceQuery(
    Guid SellerId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<List<CategoryPerformanceDto>>;
