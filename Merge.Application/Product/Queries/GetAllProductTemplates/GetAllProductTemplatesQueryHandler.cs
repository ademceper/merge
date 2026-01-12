using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetAllProductTemplates;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAllProductTemplatesQueryHandler : IRequestHandler<GetAllProductTemplatesQuery, IEnumerable<ProductTemplateDto>>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetAllProductTemplatesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_TEMPLATES_ACTIVE = "product_templates_active";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30); // Templates change less frequently

    public GetAllProductTemplatesQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetAllProductTemplatesQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<ProductTemplateDto>> Handle(GetAllProductTemplatesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all product templates. CategoryId: {CategoryId}, IsActive: {IsActive}",
            request.CategoryId, request.IsActive);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        string cacheKey;
        if (request.CategoryId.HasValue)
        {
            cacheKey = $"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{request.CategoryId.Value}_{request.IsActive}";
        }
        else if (request.IsActive.HasValue && request.IsActive.Value)
        {
            cacheKey = CACHE_KEY_TEMPLATES_ACTIVE;
        }
        else
        {
            cacheKey = CACHE_KEY_ALL_TEMPLATES;
        }

        var cachedTemplates = await _cache.GetAsync<IEnumerable<ProductTemplateDto>>(cacheKey, cancellationToken);
        if (cachedTemplates != null)
        {
            _logger.LogInformation("Product templates retrieved from cache. CategoryId: {CategoryId}, IsActive: {IsActive}",
                request.CategoryId, request.IsActive);
            return cachedTemplates;
        }

        _logger.LogInformation("Cache miss for product templates. Fetching from database.");

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        IQueryable<ProductTemplate> query = _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == request.IsActive.Value);
        }

        var templates = await query
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var templateDtos = _mapper.Map<IEnumerable<ProductTemplateDto>>(templates).ToList();

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        await _cache.SetAsync(cacheKey, templateDtos, CACHE_EXPIRATION, cancellationToken);

        _logger.LogInformation("Retrieved all product templates. Count: {Count}, CategoryId: {CategoryId}, IsActive: {IsActive}",
            templates.Count, request.CategoryId, request.IsActive);

        return templateDtos;
    }
}
