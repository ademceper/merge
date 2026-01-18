using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetReferralPerformance;

public record GetReferralPerformanceQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<ReferralPerformanceDto>;

