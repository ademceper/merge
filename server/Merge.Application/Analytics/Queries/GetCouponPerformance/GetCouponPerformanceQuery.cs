using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetCouponPerformance;

public record GetCouponPerformanceQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<List<CouponPerformanceDto>>;

