using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetPopularProductTemplates;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPopularProductTemplatesQueryHandler : IRequestHandler<GetPopularProductTemplatesQuery, IEnumerable<ProductTemplateDto>>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetPopularProductTemplatesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30); // Templates change less frequently

    public GetPopularProductTemplatesQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetPopularProductTemplatesQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<IEnumerable<ProductTemplateDto>> Handle(GetPopularProductTemplatesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
        var limit = request.Limit > _paginationSettings.MaxPageSize
            ? _paginationSettings.MaxPageSize
            : request.Limit;
        if (limit < 1) limit = _paginationSettings.DefaultPageSize;

        _logger.LogInformation("Fetching popular product templates. Limit: {Limit}", limit);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_POPULAR_TEMPLATES}{limit}";
        var cachedTemplates = await _cache.GetAsync<IEnumerable<ProductTemplateDto>>(cacheKey, cancellationToken);
        if (cachedTemplates != null)
        {
            _logger.LogInformation("Popular product templates retrieved from cache. Limit: {Limit}", limit);
            return cachedTemplates;
        }

        _logger.LogInformation("Cache miss for popular product templates. Fetching from database.");

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var templates = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var templateDtos = _mapper.Map<IEnumerable<ProductTemplateDto>>(templates).ToList();

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        await _cache.SetAsync(cacheKey, templateDtos, CACHE_EXPIRATION, cancellationToken);

        _logger.LogInformation("Retrieved popular product templates. Count: {Count}, Limit: {Limit}", templates.Count, limit);

        return templateDtos;
    }
}
