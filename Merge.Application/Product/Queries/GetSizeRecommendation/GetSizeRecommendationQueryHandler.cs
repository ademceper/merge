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

namespace Merge.Application.Product.Queries.GetSizeRecommendation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetSizeRecommendationQueryHandler : IRequestHandler<GetSizeRecommendationQuery, SizeRecommendationDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetSizeRecommendationQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_SIZE_RECOMMENDATION = "size_recommendation_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30); // Recommendations change less frequently

    public GetSizeRecommendationQueryHandler(
        IDbContext context,
        ILogger<GetSizeRecommendationQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<SizeRecommendationDto> Handle(GetSizeRecommendationQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching size recommendation. ProductId: {ProductId}, Height: {Height}, Weight: {Weight}",
            request.ProductId, request.Height, request.Weight);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        // Note: Cache key includes measurements for accurate caching
        var cacheKey = $"{CACHE_KEY_SIZE_RECOMMENDATION}{request.ProductId}_{request.Height}_{request.Weight}_{request.Chest}_{request.Waist}";
        var cachedRecommendation = await _cache.GetAsync<SizeRecommendationDto>(cacheKey, cancellationToken);
        if (cachedRecommendation != null)
        {
            _logger.LogInformation("Size recommendation retrieved from cache. ProductId: {ProductId}", request.ProductId);
            return cachedRecommendation;
        }

        _logger.LogInformation("Cache miss for size recommendation. Fetching from database.");

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var productSizeGuide = await _context.Set<ProductSizeGuide>()
            .AsNoTracking()
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Entries)
            .FirstOrDefaultAsync(psg => psg.ProductId == request.ProductId, cancellationToken);

        if (productSizeGuide == null)
        {
            _logger.LogWarning("Product size guide not found. ProductId: {ProductId}", request.ProductId);
            // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
            var noGuideResult = new SizeRecommendationDto(
                RecommendedSize: "N/A",
                Confidence: "Low",
                AlternativeSizes: Array.Empty<string>(),
                Reasoning: "No size guide available for this product"
            );
            // Cache negative result for a short time
            await _cache.SetAsync(cacheKey, noGuideResult, TimeSpan.FromMinutes(5), cancellationToken);
            return noGuideResult;
        }

        var entries = productSizeGuide.SizeGuide.Entries
            .OrderBy(e => e.DisplayOrder)
            .ToList();

        SizeGuideEntry? bestMatch = null;
        decimal bestScore = decimal.MaxValue;
        var alternativeSizes = new List<SizeGuideEntry>();

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
                if (score < 20)
                {
                    alternativeSizes.Add(entry);
                }
            }
        }

        // Sort alternatives by score and take top 3
        alternativeSizes = alternativeSizes
            .OrderBy(e => e.DisplayOrder)
            .Take(3)
            .ToList();

        SizeRecommendationDto recommendation;
        if (bestMatch != null)
        {
            string confidence = bestScore < 5 ? "High" : bestScore < 10 ? "Medium" : "Low";
            // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
            recommendation = new SizeRecommendationDto(
                RecommendedSize: bestMatch.SizeLabel,
                Confidence: confidence,
                AlternativeSizes: alternativeSizes.Select(s => s.SizeLabel).ToList().AsReadOnly(),
                Reasoning: $"Based on your measurements (Height: {request.Height}, Weight: {request.Weight})"
            );
        }
        else
        {
            // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
            recommendation = new SizeRecommendationDto(
                RecommendedSize: "N/A",
                Confidence: "Low",
                AlternativeSizes: Array.Empty<string>(),
                Reasoning: "Unable to match your measurements with available sizes"
            );
        }

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        await _cache.SetAsync(cacheKey, recommendation, CACHE_EXPIRATION, cancellationToken);

        _logger.LogInformation("Size recommendation generated. ProductId: {ProductId}, RecommendedSize: {RecommendedSize}, Confidence: {Confidence}",
            request.ProductId, recommendation.RecommendedSize, recommendation.Confidence);

        return recommendation;
    }
}
