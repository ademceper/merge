using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Queries.GetPendingPolicies;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPendingPoliciesQueryHandler : IRequestHandler<GetPendingPoliciesQuery, IEnumerable<PolicyDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPendingPoliciesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PENDING_POLICIES = "pending_policies_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetPendingPoliciesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetPendingPoliciesQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<PolicyDto>> Handle(GetPendingPoliciesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving pending policies. UserId: {UserId}", request.UserId);

        var cacheKey = $"{CACHE_KEY_PENDING_POLICIES}{request.UserId}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedPolicies = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for pending policies. UserId: {UserId}", request.UserId);

                // ✅ PERFORMANCE: Database'de filtering yap (memory'de işlem YASAK)
                // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
                var pendingPolicies = await _context.Set<Policy>()
                    .AsNoTracking()
                    .Include(p => p.CreatedBy)
                    .Where(p => p.IsActive && 
                           p.RequiresAcceptance &&
                           (p.EffectiveDate == null || p.EffectiveDate <= DateTime.UtcNow) &&
                           (p.ExpiryDate == null || p.ExpiryDate >= DateTime.UtcNow) &&
                           !_context.Set<PolicyAcceptance>()
                               .Any(pa => pa.UserId == request.UserId && 
                                         pa.PolicyId == p.Id && 
                                         pa.AcceptedVersion == p.Version && 
                                         pa.IsActive))
                    .OrderByDescending(p => p.Version)
                    .Take(100) // ✅ Güvenlik: Maksimum 100 pending policy
                    .ToListAsync(cancellationToken);

                // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al
                var policyIds = pendingPolicies.Select(p => p.Id).ToList();
                var acceptanceCounts = policyIds.Count > 0
                    ? await _context.Set<PolicyAcceptance>()
                        .AsNoTracking()
                        .Where(pa => policyIds.Contains(pa.PolicyId) && pa.IsActive)
                        .GroupBy(pa => pa.PolicyId)
                        .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.PolicyId, x => x.Count, cancellationToken)
                    : new Dictionary<Guid, int>();

                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
                // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
                var result = new List<PolicyDto>(pendingPolicies.Count);
                foreach (var policy in pendingPolicies)
                {
                    var dto = _mapper.Map<PolicyDto>(policy);
                    // ✅ BOLUM 7.1.5: Records - with expression kullanımı
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

