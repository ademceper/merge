using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Domain.Entities;
using System.Text.Json;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Application.DTOs.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Product;

public class ProductComparisonService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductComparisonService> logger) : IProductComparisonService
{

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<ProductComparisonDto> CreateComparisonAsync(Guid userId, CreateComparisonDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product comparison oluşturuluyor. UserId: {UserId}, ProductCount: {ProductCount}",
            userId, dto.ProductIds.Count);

        if (dto.ProductIds.Count > 5)
        {
            throw new ValidationException("Aynı anda en fazla 5 ürün karşılaştırılabilir.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var comparison = ProductComparison.Create(
            userId: userId,
            name: dto.Name ?? "Unnamed Comparison",
            isSaved: !string.IsNullOrEmpty(dto.Name));

        await context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = dto.ProductIds.Distinct().ToList();
        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        int position = 0;
        foreach (var productId in dto.ProductIds)
        {
            if (products.ContainsKey(productId))
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
                var item = ProductComparisonItem.Create(
                    comparisonId: comparison.Id,
                    productId: productId,
                    position: position++);

                await context.Set<ProductComparisonItem>().AddAsync(item, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparison.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product comparison oluşturuldu. ComparisonId: {ComparisonId}, UserId: {UserId}",
            comparison!.Id, userId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return await MapToDto(comparison, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductComparisonDto?> GetComparisonAsync(Guid id, CancellationToken cancellationToken = default)
    {

        var comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return comparison != null ? await MapToDto(comparison, cancellationToken) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductComparisonDto?> GetUserComparisonAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            // Create a new temporary comparison
            comparison = ProductComparison.Create(
                userId: userId,
                name: "Current Comparison",
                isSaved: false);

            await context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return await MapToDto(comparison, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ProductComparisonDto>> GetUserComparisonsAsync(Guid userId, bool savedOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (nested ThenInclude)
        var query = context.Set<ProductComparison>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Where(c => c.UserId == userId);

        if (savedOnly)
        {
            query = query.Where(c => c.IsSaved);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var comparisons = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: MapToDto metodu hala kullanılıyor çünkü Products listesi manuel set ediliyor
        var dtos = new List<ProductComparisonDto>(comparisons.Count);
        foreach (var comparison in comparisons)
        {
            dtos.Add(await MapToDto(comparison, cancellationToken));
        }

        return new PagedResult<ProductComparisonDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductComparisonDto?> GetComparisonByShareCodeAsync(string shareCode, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (nested ThenInclude)
        var comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.ShareCode == shareCode, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return comparison != null ? await MapToDto(comparison, cancellationToken) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductComparisonDto> AddProductToComparisonAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            comparison = ProductComparison.Create(
                userId: userId,
                name: "Current Comparison",
                isSaved: false);

            await context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (comparison.Items.Count >= 5)
        {
            throw new ValidationException("Aynı anda en fazla 5 ürün karşılaştırılabilir.");
        }

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var existingItem = comparison.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            throw new BusinessException("Ürün zaten karşılaştırmada.");
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var item = ProductComparisonItem.Create(
            comparisonId: comparison.Id,
            productId: productId,
            position: comparison.Items.Count);

        await context.Set<ProductComparisonItem>().AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparison.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return await MapToDto(comparison!, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RemoveProductFromComparisonAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison == null) return false;

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var item = comparison.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return false;

        item.MarkAsDeleted();

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        // Reorder remaining items
        var remainingItems = comparison.Items
            .OrderBy(i => i.Position)
            .ToList();

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        for (int i = 0; i < remainingItems.Count; i++)
        {
            remainingItems[i].UpdatePosition(i);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SaveComparisonAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await context.Set<ProductComparison>()
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        comparison.Save(name);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<string> GenerateShareCodeAsync(Guid comparisonId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await context.Set<ProductComparison>()
            .FirstOrDefaultAsync(c => c.Id == comparisonId, cancellationToken);

        if (comparison == null)
        {
            throw new NotFoundException("Karşılaştırma", comparisonId);
        }

        if (!string.IsNullOrEmpty(comparison.ShareCode))
        {
            return comparison.ShareCode;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        comparison.GenerateShareCode();
        var shareCode = comparison.ShareCode!;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return shareCode;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ClearComparisonAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison == null) return false;

        foreach (var item in comparison.Items)
        {
            item.MarkAsDeleted();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteComparisonAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await context.Set<ProductComparison>()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

        if (comparison == null) return false;

        comparison.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ComparisonMatrixDto> GetComparisonMatrixAsync(Guid comparisonId, CancellationToken cancellationToken = default)
    {

        var comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparisonId, cancellationToken);

        if (comparison == null)
        {
            throw new NotFoundException("Karşılaştırma", comparisonId);
        }

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var productIds = comparison.Items
            .OrderBy(i => i.Position)
            .Select(i => i.ProductId)
            .ToList();

        // ✅ PERFORMANCE: Batch load reviews to avoid N+1 queries
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

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var attributeNames = new List<string>
        {
            "Price",
            "Stock",
            "Rating",
            "Reviews",
            "Brand",
            "Category"
        };
        var matrix = new ComparisonMatrixDto(
            AttributeNames: attributeNames,
            Products: new List<ComparisonProductDto>(),
            AttributeValues: new Dictionary<string, IReadOnlyList<string>>());

        var comparisonProducts = new List<ComparisonProductDto>();
        var attributeValues = new Dictionary<string, List<string>>();

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var products = comparison.Items
            .OrderBy(i => i.Position)
            .Select(i => i.Product)
            .ToList();

        foreach (var product in products)
        {
            var reviewStats = reviewsDict.TryGetValue(product.Id, out var stats) ? stats : null;

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
            var compProduct = mapper.Map<ComparisonProductDto>(product) with
            {
                Rating = reviewStats != null ? (decimal?)reviewStats.Rating : null,
                ReviewCount = reviewStats?.Count ?? 0,
                Specifications = new Dictionary<string, string>(), // TODO: Map from product specifications
                Features = new List<string>() // TODO: Map from product features
            };
            comparisonProducts.Add(compProduct);
        }

        // Build attribute value matrix
        attributeValues["Price"] = comparisonProducts.Select(p =>
            p.DiscountPrice.HasValue ? $"${p.DiscountPrice:F2}" : $"${p.Price:F2}").ToList();

        attributeValues["Stock"] = comparisonProducts.Select(p =>
            p.StockQuantity > 0 ? $"{p.StockQuantity} in stock" : "Out of stock").ToList();

        attributeValues["Rating"] = comparisonProducts.Select(p =>
            p.Rating.HasValue ? $"{p.Rating:F1} stars" : "No ratings").ToList();

        attributeValues["Reviews"] = comparisonProducts.Select(p =>
            $"{p.ReviewCount} reviews").ToList();

        attributeValues["Brand"] = comparisonProducts.Select(p => p.Brand).ToList();

        attributeValues["Category"] = comparisonProducts.Select(p => p.Category).ToList();

        // Add specification attributes
        var allSpecKeys = comparisonProducts
            .SelectMany(p => p.Specifications.Keys)
            .Distinct()
            .ToList();

        foreach (var key in allSpecKeys)
        {
            if (!attributeNames.Contains(key))
            {
                attributeNames.Add(key);
                attributeValues[key] = comparisonProducts
                    .Select(p => p.Specifications.ContainsKey(key) ? p.Specifications[key] : "N/A")
                    .ToList();
            }
        }

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        return new ComparisonMatrixDto(
            AttributeNames: attributeNames,
            Products: comparisonProducts,
            AttributeValues: attributeValues.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<string>)kv.Value));
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        var itemsQuery = context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position);

        var items = await itemsQuery
            .AsSplitQuery()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load reviews to avoid N+1 queries (subquery ile)
        var productIdsSubquery = from i in itemsQuery select i.ProductId;
        Dictionary<Guid, (decimal Rating, int Count)> reviewsDict;
        var reviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => productIdsSubquery.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Rating = (decimal)g.Average(r => r.Rating),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);
        reviewsDict = reviews.ToDictionary(x => x.ProductId, x => (x.Rating, x.Count));

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: ComparisonProductDto için AutoMapper mapping'i eklenmeli
        var products = new List<ComparisonProductDto>();

        foreach (var item in items)
        {
            var hasReviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
            // Not: ProductComparisonItem → ComparisonProductDto mapping'i eklenmeli
            var compProduct = mapper.Map<ComparisonProductDto>(item.Product) with
            {
                Position = item.Position,
                Rating = hasReviewStats ? (decimal?)stats.Rating : null,
                ReviewCount = hasReviewStats ? stats.Count : 0,
                Specifications = new Dictionary<string, string>(), // TODO: Map from product specifications
                Features = new List<string>() // TODO: Map from product features
            };
            products.Add(compProduct);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
        var comparisonDto = mapper.Map<ProductComparisonDto>(comparison) with { Products = products };
        return comparisonDto;
    }

    private string GenerateUniqueShareCode()
    {
        // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
        var random = Random.Shared;
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
