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

public class GetActivePolicyQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetActivePolicyQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetActivePolicyQuery, PolicyDto?>
{
    private const string CACHE_KEY_ACTIVE_POLICY = "policy_active_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public async Task<PolicyDto?> Handle(GetActivePolicyQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving active policy. PolicyType: {PolicyType}, Language: {Language}",
            request.PolicyType, request.Language);

        var cacheKey = $"{CACHE_KEY_ACTIVE_POLICY}{request.PolicyType}_{request.Language}";

        var cachedPolicy = await cache.GetOrCreateNullableAsync<PolicyDto>(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for active policy. PolicyType: {PolicyType}, Language: {Language}",
                    request.PolicyType, request.Language);

                var policy = await context.Set<Policy>()
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
                    logger.LogWarning("Active policy not found. PolicyType: {PolicyType}, Language: {Language}",
                        request.PolicyType, request.Language);
                    return null;
                }

                var policyDto = mapper.Map<PolicyDto>(policy);
                
                var acceptanceCount = await context.Set<PolicyAcceptance>()
                    .AsNoTracking()
                    .CountAsync(pa => pa.PolicyId == policy.Id && pa.IsActive, cancellationToken);
                
                policyDto = policyDto with { AcceptanceCount = acceptanceCount };

                return policyDto;
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedPolicy;
    }
}

