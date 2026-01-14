using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetReferralPerformance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReferralPerformanceQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<ReferralPerformanceDto>;

