using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
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

namespace Merge.Application.Product.Queries.GetAllProductTemplates;

public class GetAllProductTemplatesQueryHandler(
    IDbContext context,
    ILogger<GetAllProductTemplatesQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetAllProductTemplatesQuery, IEnumerable<ProductTemplateDto>>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_TEMPLATES_ACTIVE = "product_templates_active";

    public async Task<IEnumerable<ProductTemplateDto>> Handle(GetAllProductTemplatesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all product templates. CategoryId: {CategoryId}, IsActive: {IsActive}",
            request.CategoryId, request.IsActive);

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

        var cachedTemplates = await cache.GetAsync<IEnumerable<ProductTemplateDto>>(cacheKey, cancellationToken);
        if (cachedTemplates != null)
        {
            logger.LogInformation("Product templates retrieved from cache. CategoryId: {CategoryId}, IsActive: {IsActive}",
                request.CategoryId, request.IsActive);
            return cachedTemplates;
        }

        logger.LogInformation("Cache miss for product templates. Fetching from database.");

        IQueryable<ProductTemplate> query = context.Set<ProductTemplate>()
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

        var templateDtos = mapper.Map<IEnumerable<ProductTemplateDto>>(templates).ToList();

        await cache.SetAsync(cacheKey, templateDtos, TimeSpan.FromMinutes(cacheConfig.ProductTemplateCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Retrieved all product templates. Count: {Count}, CategoryId: {CategoryId}, IsActive: {IsActive}",
            templates.Count, request.CategoryId, request.IsActive);

        return templateDtos;
    }
}
