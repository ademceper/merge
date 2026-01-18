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

    public async Task<ProductComparisonDto> CreateComparisonAsync(Guid userId, CreateComparisonDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Product comparison oluşturuluyor. UserId: {UserId}, ProductCount: {ProductCount}",
            userId, dto.ProductIds.Count);

        if (dto.ProductIds.Count > 5)
        {
            throw new ValidationException("Aynı anda en fazla 5 ürün karşılaştırılabilir.");
        }

        var comparison = ProductComparison.Create(
            userId: userId,
            name: dto.Name ?? "Unnamed Comparison",
            isSaved: !string.IsNullOrEmpty(dto.Name));

        await context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
                var item = ProductComparisonItem.Create(
                    comparisonId: comparison.Id,
                    productId: productId,
                    position: position++);

                await context.Set<ProductComparisonItem>().AddAsync(item, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparison.Id, cancellationToken);

        logger.LogInformation(
            "Product comparison oluşturuldu. ComparisonId: {ComparisonId}, UserId: {UserId}",
            comparison!.Id, userId);

        return await MapToDto(comparison, cancellationToken);
    }

    public async Task<ProductComparisonDto?> GetComparisonAsync(Guid id, CancellationToken cancellationToken = default)
    {

        var comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return comparison is not null ? await MapToDto(comparison, cancellationToken) : null;
    }

    public async Task<ProductComparisonDto?> GetUserComparisonAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison is null)
        {
            // Create a new temporary comparison
            comparison = ProductComparison.Create(
                userId: userId,
                name: "Current Comparison",
                isSaved: false);

            await context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await MapToDto(comparison, cancellationToken);
    }

    public async Task<PagedResult<ProductComparisonDto>> GetUserComparisonsAsync(Guid userId, bool savedOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

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

    public async Task<ProductComparisonDto?> GetComparisonByShareCodeAsync(string shareCode, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.ShareCode == shareCode, cancellationToken);

        return comparison is not null ? await MapToDto(comparison, cancellationToken) : null;
    }

    public async Task<ProductComparisonDto> AddProductToComparisonAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison is null)
        {
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

        var existingItem = comparison.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            throw new BusinessException("Ürün zaten karşılaştırmada.");
        }

        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        var item = ProductComparisonItem.Create(
            comparisonId: comparison.Id,
            productId: productId,
            position: comparison.Items.Count);

        await context.Set<ProductComparisonItem>().AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparison.Id, cancellationToken);

        return await MapToDto(comparison!, cancellationToken);
    }

    public async Task<bool> RemoveProductFromComparisonAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison is null) return false;

        var item = comparison.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return false;

        item.MarkAsDeleted();

        // Reorder remaining items
        var remainingItems = comparison.Items
            .OrderBy(i => i.Position)
            .ToList();

        for (int i = 0; i < remainingItems.Count; i++)
        {
            remainingItems[i].UpdatePosition(i);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SaveComparisonAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison is null) return false;

        comparison.Save(name);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<string> GenerateShareCodeAsync(Guid comparisonId, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .FirstOrDefaultAsync(c => c.Id == comparisonId, cancellationToken);

        if (comparison is null)
        {
            throw new NotFoundException("Karşılaştırma", comparisonId);
        }

        if (!string.IsNullOrEmpty(comparison.ShareCode))
        {
            return comparison.ShareCode;
        }

        comparison.GenerateShareCode();
        var shareCode = comparison.ShareCode!;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return shareCode;
    }

    public async Task<bool> ClearComparisonAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (comparison is null) return false;

        foreach (var item in comparison.Items)
        {
            item.MarkAsDeleted();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteComparisonAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

        if (comparison is null) return false;

        comparison.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ComparisonMatrixDto> GetComparisonMatrixAsync(Guid comparisonId, CancellationToken cancellationToken = default)
    {
        var comparison = await context.Set<ProductComparison>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparisonId, cancellationToken);

        if (comparison is null)
        {
            throw new NotFoundException("Karşılaştırma", comparisonId);
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
        var matrix = new ComparisonMatrixDto(
            AttributeNames: attributeNames,
            Products: new List<ComparisonProductDto>(),
            AttributeValues: new Dictionary<string, IReadOnlyList<string>>());

        List<ComparisonProductDto> comparisonProducts = [];
        var attributeValues = new Dictionary<string, List<string>>();

        var products = comparison.Items
            .OrderBy(i => i.Position)
            .Select(i => i.Product)
            .ToList();

        foreach (var product in products)
        {
            var reviewStats = reviewsDict.TryGetValue(product.Id, out var stats) ? stats : null;

            var compProduct = mapper.Map<ComparisonProductDto>(product) with
            {
                Rating = reviewStats is not null ? (decimal?)reviewStats.Rating : null,
                ReviewCount = reviewStats?.Count ?? 0,
                Specifications = new Dictionary<string, string>(), // TODO: Map from product specifications
                Features = [] // TODO: Map from product features
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

        return new ComparisonMatrixDto(
            AttributeNames: attributeNames,
            Products: comparisonProducts,
            AttributeValues: attributeValues.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<string>)kv.Value));
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken = default)
    {
        var itemsQuery = context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position);

        var items = await itemsQuery
            .AsSplitQuery()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .ToListAsync(cancellationToken);

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

        // Not: ComparisonProductDto için AutoMapper mapping'i eklenmeli
        List<ComparisonProductDto> products = [];

        foreach (var item in items)
        {
            var hasReviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats);

            // Not: ProductComparisonItem → ComparisonProductDto mapping'i eklenmeli
            var compProduct = mapper.Map<ComparisonProductDto>(item.Product) with
            {
                Position = item.Position,
                Rating = hasReviewStats ? (decimal?)stats.Rating : null,
                ReviewCount = hasReviewStats ? stats.Count : 0,
                Specifications = new Dictionary<string, string>(), // TODO: Map from product specifications
                Features = [] // TODO: Map from product features
            };
            products.Add(compProduct);
        }

        var comparisonDto = mapper.Map<ProductComparisonDto>(comparison) with { Products = products };
        return comparisonDto;
    }

    private string GenerateUniqueShareCode()
    {
        var random = Random.Shared;
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
