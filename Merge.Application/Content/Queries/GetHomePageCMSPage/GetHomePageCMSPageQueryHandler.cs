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

namespace Merge.Application.Content.Queries.GetHomePageCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetHomePageCMSPageQueryHandler : IRequestHandler<GetHomePageCMSPageQuery, CMSPageDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetHomePageCMSPageQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_HOME_PAGE = "cms_home_page";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public GetHomePageCMSPageQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetHomePageCMSPageQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<CMSPageDto?> Handle(GetHomePageCMSPageQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving home page CMS page");

        // ✅ BOLUM 10.2: Redis distributed cache for home page
        var cachedPage = await _cache.GetAsync<CMSPageDto>(CACHE_KEY_HOME_PAGE, cancellationToken);
        if (cachedPage != null)
        {
            _logger.LogInformation("Cache hit for home page");
            return cachedPage;
        }

        _logger.LogInformation("Cache miss for home page");

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.IsHomePage && p.Status == ContentStatus.Published, cancellationToken);

        if (page == null)
        {
            _logger.LogWarning("Home page not found");
            return null;
        }

        _logger.LogInformation("Successfully retrieved home page {PageId} with Title: {Title}",
            page.Id, page.Title);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var pageDto = _mapper.Map<CMSPageDto>(page);
        
        // Cache the result
        await _cache.SetAsync(CACHE_KEY_HOME_PAGE, pageDto, CACHE_EXPIRATION, cancellationToken);

        return pageDto;
    }
}

