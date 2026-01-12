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

namespace Merge.Application.Governance.Queries.GetActivePolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetActivePolicyQueryHandler : IRequestHandler<GetActivePolicyQuery, PolicyDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActivePolicyQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ACTIVE_POLICY = "policy_active_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public GetActivePolicyQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetActivePolicyQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<PolicyDto?> Handle(GetActivePolicyQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving active policy. PolicyType: {PolicyType}, Language: {Language}",
            request.PolicyType, request.Language);

        var cacheKey = $"{CACHE_KEY_ACTIVE_POLICY}{request.PolicyType}_{request.Language}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedPolicy = await _cache.GetOrCreateNullableAsync<PolicyDto>(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for active policy. PolicyType: {PolicyType}, Language: {Language}",
                    request.PolicyType, request.Language);

                var policy = await _context.Set<Policy>()
                    .AsNoTracking()
                    .Include(p => p.CreatedBy)
                    .Where(p => p.PolicyType == request.PolicyType && 
                           p.Language == request.Language && 
                           p.IsActive &&
                           (p.EffectiveDate == null || p.EffectiveDate <= DateTime.UtcNow) &&
                           (p.ExpiryDate == null || p.ExpiryDate >= DateTime.UtcNow))
                    .OrderByDescending(p => p.Version)
                    .FirstOrDefaultAsync(cancellationToken);

                if (policy == null)
                {
                    _logger.LogWarning("Active policy not found. PolicyType: {PolicyType}, Language: {Language}",
                        request.PolicyType, request.Language);
                    return null;
                }

                var policyDto = _mapper.Map<PolicyDto>(policy);
                
                // ✅ PERFORMANCE: AcceptanceCount database'de hesapla
                var acceptanceCount = await _context.Set<PolicyAcceptance>()
                    .AsNoTracking()
                    .CountAsync(pa => pa.PolicyId == policy.Id && pa.IsActive, cancellationToken);
                
                // ✅ BOLUM 7.1.5: Records - with expression kullanımı
                policyDto = policyDto with { AcceptanceCount = acceptanceCount };

                return policyDto;
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedPolicy;
    }
}

