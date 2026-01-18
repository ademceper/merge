using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetTrendingProducts;

public class GetTrendingProductsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetTrendingProductsQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetTrendingProductsQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetTrendingProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Trending products isteniyor. Days: {Days}, MaxResults: {MaxResults}",
            request.Days, request.MaxResults);

        var days = request.Days < 1 ? searchConfig.DefaultTrendingDays : request.Days;
        if (days > searchConfig.MaxTrendingDays) days = searchConfig.MaxTrendingDays;

        var maxResults = request.MaxResults > searchConfig.MaxRecommendationResults
            ? searchConfig.MaxRecommendationResults
            : request.MaxResults;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var trendingProducts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.CreatedAt >= startDate)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                RecommendationScore = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.RecommendationScore)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        if (trendingProducts.Count == 0)
        {
            return Array.Empty<ProductRecommendationDto>();
        }

        var productIds = trendingProducts.Select(tp => tp.ProductId).ToList();
        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        List<ProductRecommendationDto> recommendations = [];
        foreach (var tp in trendingProducts)
        {
            if (products.TryGetValue(tp.ProductId, out var product))
            {
                var rec = mapper.Map<ProductRecommendationDto>(product);
                recommendations.Add(new ProductRecommendationDto(
                    rec.ProductId,
                    rec.Name,
                    rec.Description,
                    rec.Price,
                    rec.DiscountPrice,
                    rec.ImageUrl,
                    rec.Rating,
                    rec.ReviewCount,
                    $"Trending in last {days} days",
                    tp.RecommendationScore
                ));
            }
        }

        logger.LogInformation(
            "Trending products tamamlandı. Days: {Days}, Count: {Count}",
            days, recommendations.Count);

        return recommendations;
    }
}
