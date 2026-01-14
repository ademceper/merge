using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetDetailedPerformanceMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetDetailedPerformanceMetricsQuery(
    Guid SellerId,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<SellerPerformanceMetricsDto>;
