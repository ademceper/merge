using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetDashboardStats;

public record GetDashboardStatsQuery() : IRequest<DashboardStatsDto>;

