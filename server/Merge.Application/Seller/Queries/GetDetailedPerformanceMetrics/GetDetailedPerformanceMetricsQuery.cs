using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetDetailedPerformanceMetrics;

public record GetDetailedPerformanceMetricsQuery(
    Guid SellerId,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<SellerPerformanceMetricsDto>;
