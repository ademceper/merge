using MediatR;

namespace Merge.Application.Analytics.Commands.RefreshDashboardMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RefreshDashboardMetricsCommand() : IRequest;

