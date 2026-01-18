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

namespace Merge.Application.Search.Queries.GetBestSellers;

public class GetBestSellersQueryHandler(IDbContext context, IMapper mapper, ILogger<GetBestSellersQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetBestSellersQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetBestSellersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Best sellers isteniyor. MaxResults: {MaxResults}",
            request.MaxResults);

        var maxResults = request.MaxResults > searchConfig.MaxRecommendationResults
            ? searchConfig.MaxRecommendationResults
            : request.MaxResults;

        var bestSellers = await context.Set<OrderItem>()
            .AsNoTracking()
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                RecommendationScore = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.RecommendationScore)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        if (bestSellers.Count == 0)
        {
            return Array.Empty<ProductRecommendationDto>();
        }

        var productIds = bestSellers.Select(bs => bs.ProductId).ToList();
        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        List<ProductRecommendationDto> recommendations = [];
        foreach (var bs in bestSellers)
        {
            if (products.TryGetValue(bs.ProductId, out var product))
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
                    "Best seller",
                    bs.RecommendationScore
                ));
            }
        }

        logger.LogInformation(
            "Best sellers tamamlandı. Count: {Count}",
            recommendations.Count);

        return recommendations;
    }
}
