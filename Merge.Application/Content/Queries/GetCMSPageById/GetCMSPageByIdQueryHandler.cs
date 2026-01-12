using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetCMSPageById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCMSPageByIdQueryHandler : IRequestHandler<GetCMSPageByIdQuery, CMSPageDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCMSPageByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_CMS_PAGE_BY_ID = "cms_page_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15); // CMS pages change less frequently

    public GetCMSPageByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCMSPageByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<CMSPageDto?> Handle(GetCMSPageByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving CMS page with Id: {PageId}", request.Id);

        var cacheKey = $"{CACHE_KEY_CMS_PAGE_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache for single CMS page
        var cachedPage = await _cache.GetAsync<CMSPageDto>(cacheKey, cancellationToken);
        if (cachedPage != null)
        {
            _logger.LogInformation("Cache hit for CMS page. PageId: {PageId}", request.Id);
            return cachedPage;
        }

        _logger.LogInformation("Cache miss for CMS page. PageId: {PageId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (page == null)
        {
            _logger.LogWarning("CMS page not found with Id: {PageId}", request.Id);
            return null;
        }

        _logger.LogInformation("Successfully retrieved CMS page {PageId} with Title: {Title}",
            request.Id, page.Title);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var pageDto = _mapper.Map<CMSPageDto>(page);
        
        // Cache the result
        await _cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);

        return pageDto;
    }
}

