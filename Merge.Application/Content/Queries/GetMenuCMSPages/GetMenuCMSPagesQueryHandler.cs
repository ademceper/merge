using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetMenuCMSPages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetMenuCMSPagesQueryHandler : IRequestHandler<GetMenuCMSPagesQuery, IEnumerable<CMSPageDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMenuCMSPagesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_MENU_PAGES = "cms_menu_pages";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public GetMenuCMSPagesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetMenuCMSPagesQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<CMSPageDto>> Handle(GetMenuCMSPagesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving menu CMS pages");

        // ✅ BOLUM 10.2: Redis distributed cache for menu pages
        var cachedPages = await _cache.GetAsync<IEnumerable<CMSPageDto>>(CACHE_KEY_MENU_PAGES, cancellationToken);
        if (cachedPages != null)
        {
            _logger.LogInformation("Cache hit for menu CMS pages");
            return cachedPages;
        }

        _logger.LogInformation("Cache miss for menu CMS pages");

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Menü sayfaları genelde sınırlı (10-20) ama güvenlik için limit ekle
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        var pages = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .Where(p => p.ShowInMenu && p.Status == ContentStatus.Published && p.ParentPageId == null)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Title)
            .Take(100) // ✅ Güvenlik: Maksimum 100 menü sayfası
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} menu CMS pages", pages.Count);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var pageDtos = new List<CMSPageDto>(pages.Count);
        foreach (var page in pages)
        {
            pageDtos.Add(_mapper.Map<CMSPageDto>(page));
        }

        // Cache the result
        await _cache.SetAsync(CACHE_KEY_MENU_PAGES, pageDtos, CACHE_EXPIRATION, cancellationToken);

        return pageDtos;
    }
}

