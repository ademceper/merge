using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.Get2FAStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record Get2FAStatsQuery() : IRequest<TwoFactorStatsDto>;

