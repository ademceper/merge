using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.DTOs.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Search;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IPersonalizationService
{
    Task<IEnumerable<ProductDto>> GetPersonalizedProductsAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetPersonalizedRecommendationsAsync(Guid userId, Guid? productId = null, int count = 10, CancellationToken cancellationToken = default);
    Task<PersonalizationProfileDto> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class PersonalizationService(IDbContext context, IMapper mapper, ILogger<PersonalizationService> logger) : IPersonalizationService
{

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductDto>> GetPersonalizedProductsAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !u.IsDeleted (Global Query Filter)
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Enumerable.Empty<ProductDto>();
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !rv.IsDeleted, !w.IsDeleted, !oi.Order.IsDeleted (Global Query Filter)
        // Kullanıcının geçmiş aktivitelerini analiz et
        var viewedProducts = await context.Set<RecentlyViewedProduct>()
            .AsNoTracking()
            .Where(rv => rv.UserId == userId)
            .OrderByDescending(rv => rv.ViewedAt)
            .Take(20)
            .Select(rv => rv.ProductId)
            .ToListAsync(cancellationToken);

        var wishlistProducts = await context.Set<Wishlist>()
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => w.ProductId)
            .ToListAsync(cancellationToken);

        var orderProducts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Order.UserId == userId)
            .Select(oi => oi.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Kategorileri analiz et
        var favoriteCategories = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Order.UserId == userId)
            .Include(oi => oi.Product)
            .GroupBy(oi => oi.Product.CategoryId)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Markaları analiz et
        var favoriteBrands = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Order.UserId == userId)
            .Include(oi => oi.Product)
            .Where(oi => !string.IsNullOrEmpty(oi.Product.Brand))
            .GroupBy(oi => oi.Product.Brand)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Kişiselleştirilmiş ürünleri getir
        var query = context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .AsQueryable();

        // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK - ama bu batch loading için gerekli (kabul edilebilir)
        // Favori kategorilerden ürünler
        if (favoriteCategories.Count > 0)
        {
            query = query.Where(p => favoriteCategories.Contains(p.CategoryId));
        }

        // Favori markalardan ürünler
        if (favoriteBrands.Count > 0)
        {
            query = query.Where(p => favoriteBrands.Contains(p.Brand));
        }

        // Görüntülenen veya sipariş verilen ürünleri hariç tut
        var excludedProductIds = viewedProducts.Concat(orderProducts).Distinct().ToList();
        if (excludedProductIds.Count > 0)
        {
            query = query.Where(p => !excludedProductIds.Contains(p.Id));
        }

        var products = await query
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductDto>> GetPersonalizedRecommendationsAsync(Guid userId, Guid? productId = null, int count = 10, CancellationToken cancellationToken = default)
    {
        if (productId.HasValue)
        {
            // Ürün bazlı öneriler (Frequently Bought Together)
            return await GetProductBasedRecommendationsAsync(productId.Value, count, cancellationToken);
        }

        // Kullanıcı bazlı öneriler
        return await GetPersonalizedProductsAsync(userId, count, cancellationToken);
    }

    private async Task<IEnumerable<ProductDto>> GetProductBasedRecommendationsAsync(Guid productId, int count, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        // Bu ürünle birlikte satın alınan ürünleri bul
        var frequentlyBoughtTogether = await (
            from oi1 in context.Set<OrderItem>().AsNoTracking()
            join oi2 in context.Set<OrderItem>().AsNoTracking()
                on oi1.OrderId equals oi2.OrderId
            where oi1.ProductId == productId && oi2.ProductId != productId
            group oi2 by oi2.ProductId into g
            orderby g.Count() descending
            select g.Key
        ).Take(count).ToListAsync(cancellationToken);

        if (frequentlyBoughtTogether.Count == 0)
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
            // Eğer birlikte satın alınan ürün yoksa, aynı kategoriden öner
            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            if (product != null)
            {
                frequentlyBoughtTogether = await context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Where(p => p.CategoryId == product.CategoryId && p.Id != productId && p.IsActive)
                    .OrderByDescending(p => p.Rating)
                    .ThenByDescending(p => p.ReviewCount)
                    .Take(count)
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);
            }
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => frequentlyBoughtTogether.Contains(p.Id) && p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PersonalizationProfileDto> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !u.IsDeleted (Global Query Filter)
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !rv.IsDeleted, !w.IsDeleted, !o.IsDeleted (Global Query Filter)
        var viewedCount = await context.Set<RecentlyViewedProduct>()
            .AsNoTracking()
            .CountAsync(rv => rv.UserId == userId, cancellationToken);

        var wishlistCount = await context.Set<Wishlist>()
            .AsNoTracking()
            .CountAsync(w => w.UserId == userId, cancellationToken);

        var orderCount = await context.Set<OrderEntity>()
            .AsNoTracking()
            .CountAsync(o => o.UserId == userId, cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        var favoriteCategories = await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Where(oi => oi.Order.UserId == userId)
            .Include(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .GroupBy(oi => oi.Product.Category)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new CategoryPreferenceDto
            {
                CategoryId = g.Key.Id,
                CategoryName = g.Key.Name,
                PurchaseCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        var favoriteBrands = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Order.UserId == userId)
            .Include(oi => oi.Product)
            .Where(oi => !string.IsNullOrEmpty(oi.Product.Brand))
            .GroupBy(oi => oi.Product.Brand)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new BrandPreferenceDto
            {
                Brand = g.Key,
                PurchaseCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        return new PersonalizationProfileDto
        {
            UserId = userId,
            ViewedProductsCount = viewedCount,
            WishlistCount = wishlistCount,
            OrderCount = orderCount,
            FavoriteCategories = favoriteCategories,
            FavoriteBrands = favoriteBrands
        };
    }
}

public class PersonalizationProfileDto
{
    public Guid UserId { get; set; }
    public int ViewedProductsCount { get; set; }
    public int WishlistCount { get; set; }
    public int OrderCount { get; set; }
    public List<CategoryPreferenceDto> FavoriteCategories { get; set; } = new();
    public List<BrandPreferenceDto> FavoriteBrands { get; set; } = new();
}

public class CategoryPreferenceDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
}

public class BrandPreferenceDto
{
    public string Brand { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
}

