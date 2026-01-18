using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetProductComparisonById;

public class GetProductComparisonByIdQueryHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<GetProductComparisonByIdQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetProductComparisonByIdQuery, ProductComparisonDto?>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";

    public async Task<ProductComparisonDto?> Handle(GetProductComparisonByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching comparison by ID. ComparisonId: {ComparisonId}", request.Id);

        var cacheKey = $"{CACHE_KEY_COMPARISON_BY_ID}{request.Id}";

        var cachedResult = await cache.GetOrCreateNullableAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for comparison by ID. Fetching from database.");

                var comparison = await context.Set<ProductComparison>()
                    .AsNoTracking()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

                if (comparison == null)
                {
                    return null;
                }

                return await MapToDto(comparison, cancellationToken);
            },
            TimeSpan.FromMinutes(cacheConfig.ProductComparisonCacheExpirationMinutes),
            cancellationToken);

        return cachedResult;
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken)
    {
        // ProductComparisonService'deki MapToDto mantığını kullan
        var itemsQuery = context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position);

        var items = await itemsQuery
            .AsSplitQuery()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .ToListAsync(cancellationToken);

        var productIdsSubquery = from i in itemsQuery select i.ProductId;
        Dictionary<Guid, (decimal Rating, int Count)> reviewsDict;
        var reviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => productIdsSubquery.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Rating = (decimal)g.Average(r => r.Rating),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);
        reviewsDict = reviews.ToDictionary(x => x.ProductId, x => (x.Rating, x.Count));

        List<ComparisonProductDto> products = [];
        foreach (var item in items)
        {
            var hasReviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats);
            var compProduct = mapper.Map<ComparisonProductDto>(item.Product);
            compProduct = compProduct with
            {
                Position = item.Position,
                Rating = hasReviewStats ? (decimal?)stats.Rating : null,
                ReviewCount = hasReviewStats ? stats.Count : 0,
                Specifications = new Dictionary<string, string>().AsReadOnly(),
                Features = Array.Empty<string>()
            };
            products.Add(compProduct);
        }

        var comparisonDto = mapper.Map<ProductComparisonDto>(comparison);
        comparisonDto = comparisonDto with { Products = products.AsReadOnly() };
        return comparisonDto;
    }
}
