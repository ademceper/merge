using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Governance.Queries.GetAcceptanceStats;

public class GetAcceptanceStatsQueryHandler(
    IDbContext context,
    ILogger<GetAcceptanceStatsQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetAcceptanceStatsQuery, Dictionary<string, int>>
{
    private const string CACHE_KEY_ACCEPTANCE_STATS = "policy_acceptance_stats";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public async Task<Dictionary<string, int>> Handle(GetAcceptanceStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving acceptance stats");

        var cachedStats = await cache.GetOrCreateAsync(
            CACHE_KEY_ACCEPTANCE_STATS,
            async () =>
            {
                logger.LogInformation("Cache miss for acceptance stats");

                var policies = await context.Set<Policy>()
                    .AsNoTracking()
                    .Select(p => new { p.Id, p.PolicyType, p.Version })
                    .ToListAsync(cancellationToken);

                if (policies.Count == 0)
                {
                    return new Dictionary<string, int>();
                }

                var policyIds = policies.Select(p => p.Id).ToList();
                
                var acceptanceCounts = await context.Set<PolicyAcceptance>()
                    .AsNoTracking()
                    .Where(pa => policyIds.Contains(pa.PolicyId) && pa.IsActive)
                    .GroupBy(pa => pa.PolicyId)
                    .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.PolicyId, x => x.Count, cancellationToken);

                var stats = new Dictionary<string, int>();
                foreach (var policy in policies)
                {
                    var count = acceptanceCounts.GetValueOrDefault(policy.Id, 0);
                    stats[$"{policy.PolicyType}_{policy.Version}"] = count;
                }

                return stats;
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedStats ?? new Dictionary<string, int>();
    }
}

