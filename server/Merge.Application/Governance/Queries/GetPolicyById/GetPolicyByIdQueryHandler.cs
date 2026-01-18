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

namespace Merge.Application.Governance.Queries.GetPolicyById;

public class GetPolicyByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetPolicyByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetPolicyByIdQuery, PolicyDto?>
{
    private const string CACHE_KEY_POLICY_BY_ID = "policy_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<PolicyDto?> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving policy with Id: {PolicyId}", request.Id);

        var cacheKey = $"{CACHE_KEY_POLICY_BY_ID}{request.Id}";

        var cachedPolicy = await cache.GetOrCreateNullableAsync<PolicyDto>(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for policy. PolicyId: {PolicyId}", request.Id);

                var policy = await context.Set<Policy>()
                    .AsNoTracking()
                    .Include(p => p.CreatedBy)
                    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

                if (policy is null)
                {
                    logger.LogWarning("Policy not found. PolicyId: {PolicyId}", request.Id);
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

