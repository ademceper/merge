using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetPerformanceMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPerformanceMetricsQuery(
    Guid SellerId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<SellerPerformanceDto>;
