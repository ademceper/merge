using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Queries.GetAcceptanceStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAcceptanceStatsQueryHandler : IRequestHandler<GetAcceptanceStatsQuery, Dictionary<string, int>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetAcceptanceStatsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ACCEPTANCE_STATS = "policy_acceptance_stats";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public GetAcceptanceStatsQueryHandler(
        IDbContext context,
        ILogger<GetAcceptanceStatsQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Dictionary<string, int>> Handle(GetAcceptanceStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving acceptance stats");

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedStats = await _cache.GetOrCreateAsync(
            CACHE_KEY_ACCEPTANCE_STATS,
            async () =>
            {
                _logger.LogInformation("Cache miss for acceptance stats");

                // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
                var policies = await _context.Set<Policy>()
                    .AsNoTracking()
                    .Select(p => new { p.Id, p.PolicyType, p.Version })
                    .ToListAsync(cancellationToken);

                if (policies.Count == 0)
                {
                    return new Dictionary<string, int>();
                }

                var policyIds = policies.Select(p => p.Id).ToList();
                
                // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al
                var acceptanceCounts = await _context.Set<PolicyAcceptance>()
                    .AsNoTracking()
                    .Where(pa => policyIds.Contains(pa.PolicyId) && pa.IsActive)
                    .GroupBy(pa => pa.PolicyId)
                    .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.PolicyId, x => x.Count, cancellationToken);

                // ✅ PERFORMANCE: Memory'de minimal işlem (sadece dictionary oluşturma)
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

