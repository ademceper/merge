using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Queries.GetFraudAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetFraudAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<FraudAnalyticsDto>;
