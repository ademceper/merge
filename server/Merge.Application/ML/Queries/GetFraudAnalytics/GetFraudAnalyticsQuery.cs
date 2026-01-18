using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Queries.GetFraudAnalytics;

public record GetFraudAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<FraudAnalyticsDto>;
