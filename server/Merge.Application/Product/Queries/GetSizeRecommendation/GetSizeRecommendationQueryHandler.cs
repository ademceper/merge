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

namespace Merge.Application.Product.Queries.GetSizeRecommendation;

public class GetSizeRecommendationQueryHandler(IDbContext context, ILogger<GetSizeRecommendationQueryHandler> logger, ICacheService cache, IOptions<CacheSettings> cacheSettings, IOptions<RecommendationSettings> recommendationSettings) : IRequestHandler<GetSizeRecommendationQuery, SizeRecommendationDto>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;
    private readonly RecommendationSettings recommendationConfig = recommendationSettings.Value;

    private const string CACHE_KEY_SIZE_RECOMMENDATION = "size_recommendation_";

    public async Task<SizeRecommendationDto> Handle(GetSizeRecommendationQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching size recommendation. ProductId: {ProductId}, Height: {Height}, Weight: {Weight}",
            request.ProductId, request.Height, request.Weight);

        // Note: Cache key includes measurements for accurate caching
        var cacheKey = $"{CACHE_KEY_SIZE_RECOMMENDATION}{request.ProductId}_{request.Height}_{request.Weight}_{request.Chest}_{request.Waist}";
        var cachedRecommendation = await cache.GetAsync<SizeRecommendationDto>(cacheKey, cancellationToken);
        if (cachedRecommendation != null)
        {
            logger.LogInformation("Size recommendation retrieved from cache. ProductId: {ProductId}", request.ProductId);
            return cachedRecommendation;
        }

        logger.LogInformation("Cache miss for size recommendation. Fetching from database.");

        var productSizeGuide = await context.Set<ProductSizeGuide>()
            .AsNoTracking()
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Entries)
            .FirstOrDefaultAsync(psg => psg.ProductId == request.ProductId, cancellationToken);

        if (productSizeGuide == null)
        {
            logger.LogWarning("Product size guide not found. ProductId: {ProductId}", request.ProductId);
            var noGuideResult = new SizeRecommendationDto(
                RecommendedSize: "N/A",
                Confidence: "Low",
                AlternativeSizes: Array.Empty<string>(),
                Reasoning: "No size guide available for this product"
            );
            // Cache negative result for a short time
            await cache.SetAsync(cacheKey, noGuideResult, TimeSpan.FromMinutes(cacheConfig.NoSizeGuideCacheExpirationMinutes), cancellationToken);
            return noGuideResult;
        }

        var entries = productSizeGuide.SizeGuide.Entries
            .OrderBy(e => e.DisplayOrder)
            .ToList();

        SizeGuideEntry? bestMatch = null;
        decimal bestScore = decimal.MaxValue;
        List<SizeGuideEntry> alternativeSizes = [];

        foreach (var entry in entries)
        {
            decimal score = 0;
            int matchCount = 0;

            if (entry.Height.HasValue)
            {
                score += Math.Abs(entry.Height.Value - request.Height);
                matchCount++;
            }

            if (entry.Weight.HasValue)
            {
                score += Math.Abs(entry.Weight.Value - request.Weight);
                matchCount++;
            }

            if (request.Chest.HasValue && entry.Chest.HasValue)
            {
                score += Math.Abs(entry.Chest.Value - request.Chest.Value);
                matchCount++;
            }

            if (request.Waist.HasValue && entry.Waist.HasValue)
            {
                score += Math.Abs(entry.Waist.Value - request.Waist.Value);
                matchCount++;
            }

            if (matchCount > 0)
            {
                score /= matchCount;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestMatch = entry;
                }
                
                // Add to alternatives if score is reasonable
                if (score < recommendationConfig.AlternativeSizeScoreThreshold)
                {
                    alternativeSizes.Add(entry);
                }
            }
        }

        // Sort alternatives by score and take top N
        alternativeSizes = alternativeSizes
            .OrderBy(e => e.DisplayOrder)
            .Take(recommendationConfig.MaxAlternativeSizesCount)
            .ToList();

        SizeRecommendationDto recommendation;
        if (bestMatch != null)
        {
            string confidence = bestScore < recommendationConfig.HighConfidenceScoreThreshold 
                ? "High" 
                : bestScore < recommendationConfig.MediumConfidenceScoreThreshold 
                    ? "Medium" 
                    : "Low";
            recommendation = new SizeRecommendationDto(
                RecommendedSize: bestMatch.SizeLabel,
                Confidence: confidence,
                AlternativeSizes: alternativeSizes.Select(s => s.SizeLabel).ToList().AsReadOnly(),
                Reasoning: $"Based on your measurements (Height: {request.Height}, Weight: {request.Weight})"
            );
        }
        else
        {
            recommendation = new SizeRecommendationDto(
                RecommendedSize: "N/A",
                Confidence: "Low",
                AlternativeSizes: Array.Empty<string>(),
                Reasoning: "Unable to match your measurements with available sizes"
            );
        }

        await cache.SetAsync(cacheKey, recommendation, TimeSpan.FromMinutes(cacheConfig.SizeRecommendationCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Size recommendation generated. ProductId: {ProductId}, RecommendedSize: {RecommendedSize}, Confidence: {Confidence}",
            request.ProductId, recommendation.RecommendedSize, recommendation.Confidence);

        return recommendation;
    }
}
