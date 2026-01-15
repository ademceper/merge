using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Governance.Queries.GetPendingPolicies;

public class GetPendingPoliciesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetPendingPoliciesQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetPendingPoliciesQuery, IEnumerable<PolicyDto>>
{
    private const string CACHE_KEY_PENDING_POLICIES = "pending_policies_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<IEnumerable<PolicyDto>> Handle(GetPendingPoliciesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving pending policies. UserId: {UserId}", request.UserId);

        var cacheKey = $"{CACHE_KEY_PENDING_POLICIES}{request.UserId}";

        var cachedPolicies = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for pending policies. UserId: {UserId}", request.UserId);

                // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
                var pendingPoliciesQuery = context.Set<Policy>()
                    .AsNoTracking()
                    .Where(p => p.IsActive && 
                           p.RequiresAcceptance &&
                           (p.EffectiveDate == null || p.EffectiveDate <= DateTime.UtcNow) &&
                           (p.ExpiryDate == null || p.ExpiryDate >= DateTime.UtcNow) &&
                           !context.Set<PolicyAcceptance>()
                               .Any(pa => pa.UserId == request.UserId && 
                                         pa.PolicyId == p.Id && 
                                         pa.AcceptedVersion == p.Version && 
                                         pa.IsActive))
                    .OrderByDescending(p => p.Version)
                    .Take(100);

                var policyIdsSubquery = from p in pendingPoliciesQuery select p.Id;
                var acceptanceCounts = await context.Set<PolicyAcceptance>()
                    .AsNoTracking()
                    .Where(pa => policyIdsSubquery.Contains(pa.PolicyId) && pa.IsActive)
                    .GroupBy(pa => pa.PolicyId)
                    .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.PolicyId, x => x.Count, cancellationToken);

                var pendingPolicies = await pendingPoliciesQuery
                    .Include(p => p.CreatedBy)
                    .ToListAsync(cancellationToken);

                var result = new List<PolicyDto>(pendingPolicies.Count);
                foreach (var policy in pendingPolicies)
                {
                    var dto = mapper.Map<PolicyDto>(policy);
                    dto = dto with { AcceptanceCount = acceptanceCounts.GetValueOrDefault(policy.Id, 0) };
                    result.Add(dto);
                }
                return result;
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedPolicies ?? Enumerable.Empty<PolicyDto>();
    }
}

