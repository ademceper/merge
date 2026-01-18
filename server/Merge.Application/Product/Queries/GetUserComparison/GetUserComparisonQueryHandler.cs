using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetUserComparison;

public class GetUserComparisonQueryHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<GetUserComparisonQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetUserComparisonQuery, ProductComparisonDto>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";

    public async Task<ProductComparisonDto> Handle(GetUserComparisonQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching user comparison. UserId: {UserId}", request.UserId);

        var cacheKey = $"{CACHE_KEY_USER_COMPARISON}{request.UserId}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for user comparison. Fetching from database.");

                var comparison = await context.Set<ProductComparison>()
                    .AsNoTracking()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Category)
                    .Where(c => c.UserId == request.UserId && !c.IsSaved)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (comparison == null)
                {
                    comparison = ProductComparison.Create(
                        request.UserId,
                        "Current Comparison",
                        false);

                    await context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return await MapToDto(comparison, cancellationToken);
            },
            TimeSpan.FromMinutes(cacheConfig.UserComparisonCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken)
    {
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
