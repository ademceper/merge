using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetPerformanceMetrics;

public record GetPerformanceMetricsQuery(
    Guid SellerId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<SellerPerformanceDto>;
