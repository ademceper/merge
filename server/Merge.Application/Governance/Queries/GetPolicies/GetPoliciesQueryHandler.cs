using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Governance.Queries.GetPolicies;

public class GetPoliciesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetPoliciesQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetPoliciesQuery, PagedResult<PolicyDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private const string CACHE_KEY_POLICIES = "policies_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<PagedResult<PolicyDto>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving policies. PolicyType: {PolicyType}, Language: {Language}, ActiveOnly: {ActiveOnly}, Page: {Page}, PageSize: {PageSize}",
            request.PolicyType, request.Language, request.ActiveOnly, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_POLICIES}{request.PolicyType ?? "all"}_{request.Language ?? "all"}_{request.ActiveOnly}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for policies. PolicyType: {PolicyType}, Language: {Language}, ActiveOnly: {ActiveOnly}, Page: {Page}, PageSize: {PageSize}",
                    request.PolicyType, request.Language, request.ActiveOnly, page, pageSize);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                IQueryable<Policy> query = context.Set<Policy>()
                    .AsNoTracking()
                    .Include(p => p.CreatedBy);

                if (!string.IsNullOrEmpty(request.PolicyType))
                {
                    query = query.Where(p => p.PolicyType == request.PolicyType);
                }

                if (!string.IsNullOrEmpty(request.Language))
                {
                    query = query.Where(p => p.Language == request.Language);
                }

                if (request.ActiveOnly)
                {
                    query = query.Where(p => p.IsActive);
                }

                var orderedQuery = query.OrderByDescending(p => p.Version).ThenByDescending(p => p.CreatedAt);
                var totalCount = await orderedQuery.CountAsync(cancellationToken);

                var paginatedPoliciesQuery = orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var policies = await paginatedPoliciesQuery.ToListAsync(cancellationToken);

                // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al (subquery ile)
                var policyIdsSubquery = from p in paginatedPoliciesQuery select p.Id;
                var acceptanceCounts = await context.Set<PolicyAcceptance>()
                    .AsNoTracking()
                    .Where(pa => policyIdsSubquery.Contains(pa.PolicyId) && pa.IsActive)
                    .GroupBy(pa => pa.PolicyId)
                    .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.PolicyId, x => x.Count, cancellationToken);

                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
                // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
                var result = new List<PolicyDto>(policies.Count);
                foreach (var policy in policies)
                {
                    var dto = mapper.Map<PolicyDto>(policy);
                    // ✅ BOLUM 7.1.5: Records - with expression kullanımı
                    dto = dto with { AcceptanceCount = acceptanceCounts.GetValueOrDefault(policy.Id, 0) };
                    result.Add(dto);
                }

                return new PagedResult<PolicyDto>
                {
                    Items = result,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedResult ?? new PagedResult<PolicyDto>
        {
            Items = new List<PolicyDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }
}

