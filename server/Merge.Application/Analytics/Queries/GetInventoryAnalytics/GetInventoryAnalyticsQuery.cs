using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetInventoryAnalytics;

public record GetInventoryAnalyticsQuery() : IRequest<InventoryAnalyticsDto>;

