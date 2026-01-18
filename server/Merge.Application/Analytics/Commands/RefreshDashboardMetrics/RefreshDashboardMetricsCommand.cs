using MediatR;

namespace Merge.Application.Analytics.Commands.RefreshDashboardMetrics;

public record RefreshDashboardMetricsCommand() : IRequest;

