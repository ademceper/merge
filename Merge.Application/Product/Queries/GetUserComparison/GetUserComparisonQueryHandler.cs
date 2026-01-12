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

namespace Merge.Application.Product.Queries.GetUserComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserComparisonQueryHandler : IRequestHandler<GetUserComparisonQuery, ProductComparisonDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetUserComparisonQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Current comparison can change frequently

    public GetUserComparisonQueryHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<GetUserComparisonQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductComparisonDto> Handle(GetUserComparisonQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user comparison. UserId: {UserId}", request.UserId);

        var cacheKey = $"{CACHE_KEY_USER_COMPARISON}{request.UserId}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for user comparison. Fetching from database.");

                var comparison = await _context.Set<ProductComparison>()
                    .AsNoTracking()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Category)
                    .Where(c => c.UserId == request.UserId && !c.IsSaved)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (comparison == null)
                {
                    // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
                    comparison = ProductComparison.Create(
                        request.UserId,
                        "Current Comparison",
                        false);

                    await _context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return await MapToDto(comparison, cancellationToken);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedResult!;
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
