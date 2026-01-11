using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Queries.GetProductTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetProductTemplateQueryHandler : IRequestHandler<GetProductTemplateQuery, ProductTemplateDto?>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetProductTemplateQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_TEMPLATE_BY_ID = "product_template_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30); // Templates change less frequently

    public GetProductTemplateQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetProductTemplateQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductTemplateDto?> Handle(GetProductTemplateQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product template by Id: {TemplateId}", request.TemplateId);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_TEMPLATE_BY_ID}{request.TemplateId}";
        var cachedTemplate = await _cache.GetAsync<ProductTemplateDto>(cacheKey, cancellationToken);
        if (cachedTemplate != null)
        {
            _logger.LogInformation("Product template retrieved from cache. TemplateId: {TemplateId}", request.TemplateId);
            return cachedTemplate;
        }

        _logger.LogInformation("Cache miss for product template. TemplateId: {TemplateId}", request.TemplateId);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var template = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Product template not found with Id: {TemplateId}", request.TemplateId);
            return null;
        }

        var templateDto = _mapper.Map<ProductTemplateDto>(template);

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        await _cache.SetAsync(cacheKey, templateDto, CACHE_EXPIRATION, cancellationToken);

        _logger.LogInformation("Product template retrieved successfully. TemplateId: {TemplateId}", request.TemplateId);

        return templateDto;
    }
}
