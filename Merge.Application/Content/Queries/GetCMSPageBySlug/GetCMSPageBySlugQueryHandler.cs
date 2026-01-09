using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Queries.GetCMSPageBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCMSPageBySlugQueryHandler : IRequestHandler<GetCMSPageBySlugQuery, CMSPageDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCMSPageBySlugQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_CMS_PAGE_BY_SLUG = "cms_page_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public GetCMSPageBySlugQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCMSPageBySlugQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<CMSPageDto?> Handle(GetCMSPageBySlugQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving CMS page with Slug: {Slug}", request.Slug);

        var cacheKey = $"{CACHE_KEY_CMS_PAGE_BY_SLUG}{request.Slug}";

        // ✅ BOLUM 10.2: Redis distributed cache for CMS page by slug
        var cachedPage = await _cache.GetAsync<CMSPageDto>(cacheKey, cancellationToken);
        if (cachedPage != null)
        {
            _logger.LogInformation("Cache hit for CMS page by slug. Slug: {Slug}", request.Slug);
            return cachedPage;
        }

        _logger.LogInformation("Cache miss for CMS page by slug. Slug: {Slug}", request.Slug);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.Status == ContentStatus.Published, cancellationToken);

        if (page == null)
        {
            _logger.LogWarning("CMS page not found with Slug: {Slug}", request.Slug);
            return null;
        }

        _logger.LogInformation("Successfully retrieved CMS page {PageId} with Slug: {Slug}",
            page.Id, request.Slug);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var pageDto = _mapper.Map<CMSPageDto>(page);
        
        // Cache the result
        await _cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);

        return pageDto;
    }
}

