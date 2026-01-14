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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetBestSellersQueryHandler : IRequestHandler<GetBestSellersQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBestSellersQueryHandler> _logger;
    private readonly SearchSettings _searchSettings;

    public GetBestSellersQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetBestSellersQueryHandler> logger,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _searchSettings = searchSettings.Value;
    }

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetBestSellersQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Best sellers isteniyor. MaxResults: {MaxResults}",
            request.MaxResults);

        var maxResults = request.MaxResults > _searchSettings.MaxRecommendationResults
            ? _searchSettings.MaxRecommendationResults
            : request.MaxResults;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var bestSellers = await _context.Set<OrderItem>()
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

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = bestSellers.Select(bs => bs.ProductId).ToList();
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = new List<ProductRecommendationDto>();
        foreach (var bs in bestSellers)
        {
            if (products.TryGetValue(bs.ProductId, out var product))
            {
                var rec = _mapper.Map<ProductRecommendationDto>(product);
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

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Best sellers tamamlandı. Count: {Count}",
            recommendations.Count);

        return recommendations;
    }
}
