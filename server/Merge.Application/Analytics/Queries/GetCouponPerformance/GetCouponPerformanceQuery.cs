using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetCouponPerformance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCouponPerformanceQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<List<CouponPerformanceDto>>;

