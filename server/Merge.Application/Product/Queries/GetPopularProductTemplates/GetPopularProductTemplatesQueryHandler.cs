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

namespace Merge.Application.Product.Queries.GetPopularProductTemplates;

public class GetPopularProductTemplatesQueryHandler(
    IDbContext context,
    ILogger<GetPopularProductTemplatesQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetPopularProductTemplatesQuery, IEnumerable<ProductTemplateDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";

    public async Task<IEnumerable<ProductTemplateDto>> Handle(GetPopularProductTemplatesQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit > paginationConfig.MaxPageSize
            ? paginationConfig.MaxPageSize
            : request.Limit;
        if (limit < 1) limit = paginationConfig.DefaultPageSize;

        logger.LogInformation("Fetching popular product templates. Limit: {Limit}", limit);

        var cacheKey = $"{CACHE_KEY_POPULAR_TEMPLATES}{limit}";
        var cachedTemplates = await cache.GetAsync<IEnumerable<ProductTemplateDto>>(cacheKey, cancellationToken);
        if (cachedTemplates is not null)
        {
            logger.LogInformation("Popular product templates retrieved from cache. Limit: {Limit}", limit);
            return cachedTemplates;
        }

        logger.LogInformation("Cache miss for popular product templates. Fetching from database.");

        var templates = await context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var templateDtos = mapper.Map<IEnumerable<ProductTemplateDto>>(templates).ToList();

        await cache.SetAsync(cacheKey, templateDtos, TimeSpan.FromMinutes(cacheConfig.PopularTemplatesCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Retrieved popular product templates. Count: {Count}, Limit: {Limit}", templates.Count, limit);

        return templateDtos;
    }
}
