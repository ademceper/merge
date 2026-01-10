using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Queries.GetAllPageBuilders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllPageBuildersQueryHandler : IRequestHandler<GetAllPageBuildersQuery, PagedResult<PageBuilderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllPageBuildersQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_PAGE_BUILDERS = "page_builders_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetAllPageBuildersQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllPageBuildersQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<PageBuilderDto>> Handle(GetAllPageBuildersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving page builders. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.Status, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_PAGE_BUILDERS}{request.Status ?? "all"}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for page builders. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
                    request.Status, page, pageSize);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                IQueryable<PageBuilder> query = _context.Set<PageBuilder>()
                    .AsNoTracking()
                    .Include(pb => pb.Author);

                if (!string.IsNullOrEmpty(request.Status))
                {
                    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
                    if (Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
                    {
                        query = query.Where(pb => pb.Status == statusEnum);
                    }
                }

                var orderedQuery = query.OrderByDescending(pb => pb.CreatedAt);
                var totalCount = await orderedQuery.CountAsync(cancellationToken);

                var pageBuilders = await orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var items = pageBuilders.Select(pb => _mapper.Map<PageBuilderDto>(pb)).ToList();

                return new PagedResult<PageBuilderDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            CACHE_EXPIRATION,
            cancellationToken);

        if (cachedResult == null)
        {
            return new PagedResult<PageBuilderDto>
            {
                Items = new List<PageBuilderDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        _logger.LogInformation("Successfully retrieved {Count} page builders (Page: {Page}, PageSize: {PageSize})",
            cachedResult.Items.Count, page, pageSize);

        return cachedResult;
    }
}

