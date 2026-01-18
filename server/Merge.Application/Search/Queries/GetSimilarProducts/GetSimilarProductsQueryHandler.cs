using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetSimilarProducts;

public class GetSimilarProductsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSimilarProductsQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetSimilarProductsQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetSimilarProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Similar products isteniyor. ProductId: {ProductId}, MaxResults: {MaxResults}",
            request.ProductId, request.MaxResults);

        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product not found. ProductId: {ProductId}", request.ProductId);
            return Array.Empty<ProductRecommendationDto>();
        }

        var maxResults = request.MaxResults > searchConfig.MaxRecommendationResults
            ? searchConfig.MaxRecommendationResults
            : request.MaxResults;

        // Find products in same category with similar price range
        var priceMin = product.Price * searchConfig.SimilarProductsPriceRangeMin;
        var priceMax = product.Price * searchConfig.SimilarProductsPriceRangeMax;

        var similarProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       p.Id != request.ProductId &&
                       p.CategoryId == product.CategoryId &&
                       p.Price >= priceMin &&
                       p.Price <= priceMax)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        var recommendations = mapper.Map<IEnumerable<ProductRecommendationDto>>(similarProducts)
            .Select(rec => new ProductRecommendationDto(
                rec.ProductId,
                rec.Name,
                rec.Description,
                rec.Price,
                rec.DiscountPrice,
                rec.ImageUrl,
                rec.Rating,
                rec.ReviewCount,
                "Similar to what you're viewing",
                rec.Rating
            ))
            .ToList();

        logger.LogInformation(
            "Similar products tamamlandı. ProductId: {ProductId}, Count: {Count}",
            request.ProductId, recommendations.Count);

        return recommendations;
    }
}
