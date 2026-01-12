using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetComparisonByShareCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetComparisonByShareCodeQueryHandler : IRequestHandler<GetComparisonByShareCodeQuery, ProductComparisonDto?>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetComparisonByShareCodeQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_COMPARISON_BY_SHARE_CODE = "comparison_by_share_code_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Shared comparisons can change

    public GetComparisonByShareCodeQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetComparisonByShareCodeQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductComparisonDto?> Handle(GetComparisonByShareCodeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching comparison by share code. ShareCode: {ShareCode}", request.ShareCode);

        var cacheKey = $"{CACHE_KEY_COMPARISON_BY_SHARE_CODE}{request.ShareCode}";

        // ✅ BOLUM 10.2: Redis distributed cache
        // ✅ FIX: CS8634 - Nullable type için GetOrCreateNullableAsync kullan
        var cachedResult = await _cache.GetOrCreateNullableAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for comparison by share code. Fetching from database.");

                var comparison = await _context.Set<ProductComparison>()
                    .AsNoTracking()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(c => c.ShareCode == request.ShareCode, cancellationToken);

                if (comparison == null)
                {
                    return null;
                }

                return await MapToDto(comparison, cancellationToken);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedResult;
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken)
    {
        var items = await _context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position)
            .ToListAsync(cancellationToken);

        var productIds = items.Select(i => i.ProductId).ToList();
        Dictionary<Guid, (decimal Rating, int Count)> reviewsDict;
        if (productIds.Any())
        {
            var reviews = await _context.Set<ReviewEntity>()
                .AsNoTracking()
                .Where(r => productIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Rating = (decimal)g.Average(r => r.Rating),
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);
            reviewsDict = reviews.ToDictionary(x => x.ProductId, x => (x.Rating, x.Count));
        }
        else
        {
            reviewsDict = new Dictionary<Guid, (decimal Rating, int Count)>();
        }

        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
        var products = new List<ComparisonProductDto>();
        foreach (var item in items)
        {
            var hasReviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats);
            var compProduct = _mapper.Map<ComparisonProductDto>(item.Product);
            compProduct = compProduct with
            {
                Position = item.Position,
                Rating = hasReviewStats ? (decimal?)stats.Rating : null,
                ReviewCount = hasReviewStats ? stats.Count : 0,
                Specifications = new Dictionary<string, string>().AsReadOnly(),
                Features = new List<string>().AsReadOnly()
            };
            products.Add(compProduct);
        }

        var comparisonDto = _mapper.Map<ProductComparisonDto>(comparison);
        comparisonDto = comparisonDto with { Products = products.AsReadOnly() };
        return comparisonDto;
    }
}
