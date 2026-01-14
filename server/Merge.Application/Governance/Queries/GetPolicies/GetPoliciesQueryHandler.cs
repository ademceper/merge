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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPoliciesQueryHandler : IRequestHandler<GetPoliciesQuery, PagedResult<PolicyDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPoliciesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_POLICIES = "policies_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetPoliciesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetPoliciesQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<PolicyDto>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving policies. PolicyType: {PolicyType}, Language: {Language}, ActiveOnly: {ActiveOnly}, Page: {Page}, PageSize: {PageSize}",
            request.PolicyType, request.Language, request.ActiveOnly, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_POLICIES}{request.PolicyType ?? "all"}_{request.Language ?? "all"}_{request.ActiveOnly}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for policies. PolicyType: {PolicyType}, Language: {Language}, ActiveOnly: {ActiveOnly}, Page: {Page}, PageSize: {PageSize}",
                    request.PolicyType, request.Language, request.ActiveOnly, page, pageSize);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                IQueryable<Policy> query = _context.Set<Policy>()
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

                var policies = await orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al
                var policyIds = policies.Select(p => p.Id).ToList();
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
                var result = new List<PolicyDto>(policies.Count);
                foreach (var policy in policies)
                {
                    var dto = _mapper.Map<PolicyDto>(policy);
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

