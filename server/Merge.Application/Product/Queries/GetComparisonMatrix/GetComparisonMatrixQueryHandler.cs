using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetComparisonMatrix;

public class GetComparisonMatrixQueryHandler(
    IDbContext context,
    ILogger<GetComparisonMatrixQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetComparisonMatrixQuery, ComparisonMatrixDto>
{
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";

    public async Task<ComparisonMatrixDto> Handle(GetComparisonMatrixQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching comparison matrix. ComparisonId: {ComparisonId}", request.ComparisonId);

        var cacheKey = $"{CACHE_KEY_COMPARISON_MATRIX}{request.ComparisonId}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for comparison matrix. Fetching from database.");

                var comparison = await context.Set<ProductComparison>()
                    .AsNoTracking()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(c => c.Id == request.ComparisonId, cancellationToken);

                if (comparison is null)
                {
                    throw new NotFoundException("Karşılaştırma", request.ComparisonId);
                }

                var productIds = comparison.Items
                    .OrderBy(i => i.Position)
                    .Select(i => i.ProductId)
                    .ToList();

                var reviewsDict = await context.Set<ReviewEntity>()
                    .AsNoTracking()
                    .Where(r => productIds.Contains(r.ProductId))
                    .GroupBy(r => r.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        Rating = g.Average(r => r.Rating),
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(x => x.ProductId, cancellationToken);

                var attributeNames = new List<string>
                {
                    "Price",
                    "Stock",
                    "Rating",
                    "Reviews",
                    "Brand",
                    "Category"
                };

                List<ComparisonProductDto> comparisonProducts = [];
                var attributeValues = new Dictionary<string, List<string>>();

                var products = comparison.Items
                    .OrderBy(i => i.Position)
                    .Select(i => i.Product)
                    .ToList();

                foreach (var product in products)
                {
                    var reviewStats = reviewsDict.TryGetValue(product.Id, out var stats) ? stats : null;
                    var compProduct = mapper.Map<ComparisonProductDto>(product);
                    compProduct = compProduct with
                    {
                        Rating = reviewStats is not null ? (decimal?)reviewStats.Rating : null,
                        ReviewCount = reviewStats?.Count ?? 0,
                        Specifications = new Dictionary<string, string>().AsReadOnly(),
                        Features = Array.Empty<string>()
                    };
                    comparisonProducts.Add(compProduct);

                    // Add attribute values
                    if (!attributeValues.ContainsKey("Price"))
                        attributeValues["Price"] = [];
                    attributeValues["Price"].Add(product.DiscountPrice?.ToString("C") ?? product.Price.ToString("C"));

                    if (!attributeValues.ContainsKey("Stock"))
                        attributeValues["Stock"] = [];
                    attributeValues["Stock"].Add(product.StockQuantity.ToString());

                    if (!attributeValues.ContainsKey("Rating"))
                        attributeValues["Rating"] = [];
                    attributeValues["Rating"].Add(reviewStats is not null ? reviewStats.Rating.ToString("F1") : "N/A");

                    if (!attributeValues.ContainsKey("Reviews"))
                        attributeValues["Reviews"] = [];
                    attributeValues["Reviews"].Add((reviewStats?.Count ?? 0).ToString());

                    if (!attributeValues.ContainsKey("Brand"))
                        attributeValues["Brand"] = [];
                    attributeValues["Brand"].Add(product.Brand);

                    if (!attributeValues.ContainsKey("Category"))
                        attributeValues["Category"] = [];
                    attributeValues["Category"].Add(product.Category?.Name ?? "N/A");
                }

                var matrix = new ComparisonMatrixDto(
                    AttributeNames: attributeNames.AsReadOnly(),
                    Products: comparisonProducts.AsReadOnly(),
                    AttributeValues: attributeValues.ToDictionary(
                        kvp => kvp.Key,
                        kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly()
                    ).AsReadOnly()
                );

                return matrix;
            },
            TimeSpan.FromMinutes(cacheSettings.Value.ComparisonMatrixCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }
}
