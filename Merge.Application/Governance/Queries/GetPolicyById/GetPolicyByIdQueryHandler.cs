using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Queries.GetPolicyById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPolicyByIdQueryHandler : IRequestHandler<GetPolicyByIdQuery, PolicyDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPolicyByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_POLICY_BY_ID = "policy_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetPolicyByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetPolicyByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<PolicyDto?> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving policy with Id: {PolicyId}", request.Id);

        var cacheKey = $"{CACHE_KEY_POLICY_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedPolicy = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for policy. PolicyId: {PolicyId}", request.Id);

                var policy = await _context.Set<Policy>()
                    .AsNoTracking()
                    .Include(p => p.CreatedBy)
                    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

                if (policy == null)
                {
                    _logger.LogWarning("Policy not found. PolicyId: {PolicyId}", request.Id);
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

