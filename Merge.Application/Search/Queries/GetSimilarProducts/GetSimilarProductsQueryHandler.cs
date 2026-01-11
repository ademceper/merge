using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Search.Queries.GetSimilarProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSimilarProductsQueryHandler : IRequestHandler<GetSimilarProductsQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSimilarProductsQueryHandler> _logger;
    private readonly SearchSettings _searchSettings;

    public GetSimilarProductsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSimilarProductsQueryHandler> logger,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _searchSettings = searchSettings.Value;
    }

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetSimilarProductsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Similar products isteniyor. ProductId: {ProductId}, MaxResults: {MaxResults}",
            request.ProductId, request.MaxResults);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found. ProductId: {ProductId}", request.ProductId);
            return Array.Empty<ProductRecommendationDto>();
        }

        var maxResults = request.MaxResults > _searchSettings.MaxRecommendationResults
            ? _searchSettings.MaxRecommendationResults
            : request.MaxResults;

        // Find products in same category with similar price range
        var priceMin = product.Price * _searchSettings.SimilarProductsPriceRangeMin;
        var priceMax = product.Price * _searchSettings.SimilarProductsPriceRangeMax;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var similarProducts = await _context.Set<ProductEntity>()
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = _mapper.Map<IEnumerable<ProductRecommendationDto>>(similarProducts)
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

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Similar products tamamlandı. ProductId: {ProductId}, Count: {Count}",
            request.ProductId, recommendations.Count);

        return recommendations;
    }
}
