using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.Get2FAStats;

public record Get2FAStatsQuery() : IRequest<TwoFactorStatsDto>;

