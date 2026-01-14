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

namespace Merge.Application.Search.Queries.GetFrequentlyBoughtTogether;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetFrequentlyBoughtTogetherQueryHandler : IRequestHandler<GetFrequentlyBoughtTogetherQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFrequentlyBoughtTogetherQueryHandler> _logger;
    private readonly SearchSettings _searchSettings;

    public GetFrequentlyBoughtTogetherQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetFrequentlyBoughtTogetherQueryHandler> logger,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _searchSettings = searchSettings.Value;
    }

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetFrequentlyBoughtTogetherQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Frequently bought together isteniyor. ProductId: {ProductId}, MaxResults: {MaxResults}",
            request.ProductId, request.MaxResults);

        var maxResults = request.MaxResults > _searchSettings.MaxRecommendationResults
            ? _searchSettings.MaxRecommendationResults
            : request.MaxResults;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var frequentlyBought = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => _context.Set<OrderItem>().Any(oi2 =>
                            oi2.OrderId == oi.OrderId &&
                            oi2.ProductId == request.ProductId))
            .Where(oi => oi.ProductId != request.ProductId)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        if (frequentlyBought.Count == 0)
        {
            return Array.Empty<ProductRecommendationDto>();
        }

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = frequentlyBought.Select(fb => fb.ProductId).ToList();
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = new List<ProductRecommendationDto>();
        foreach (var fb in frequentlyBought)
        {
            if (products.TryGetValue(fb.ProductId, out var product))
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
                    "Frequently bought together",
                    fb.Count
                ));
            }
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Frequently bought together tamamlandı. ProductId: {ProductId}, Count: {Count}",
            request.ProductId, recommendations.Count);

        return recommendations;
    }
}
