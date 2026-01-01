using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Application.DTOs.Product;


namespace Merge.Application.Services.Search;

public interface IPersonalizationService
{
    Task<IEnumerable<ProductDto>> GetPersonalizedProductsAsync(Guid userId, int count = 10);
    Task<IEnumerable<ProductDto>> GetPersonalizedRecommendationsAsync(Guid userId, Guid? productId = null, int count = 10);
    Task<PersonalizationProfileDto> GetUserProfileAsync(Guid userId);
}

public class PersonalizationService : IPersonalizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PersonalizationService> _logger;

    public PersonalizationService(ApplicationDbContext context, IMapper mapper, ILogger<PersonalizationService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetPersonalizedProductsAsync(Guid userId, int count = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Enumerable.Empty<ProductDto>();
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !rv.IsDeleted, !w.IsDeleted, !oi.Order.IsDeleted (Global Query Filter)
        // Kullanıcının geçmiş aktivitelerini analiz et
        var viewedProducts = await _context.RecentlyViewedProducts
            .AsNoTracking()
            .Where(rv => rv.UserId == userId)
            .OrderByDescending(rv => rv.ViewedAt)
            .Take(20)
            .Select(rv => rv.ProductId)
            .ToListAsync();

        var wishlistProducts = await _context.Wishlists
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => w.ProductId)
            .ToListAsync();

        var orderProducts = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.UserId == userId)
            .Select(oi => oi.ProductId)
            .Distinct()
            .ToListAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Kategorileri analiz et
        var favoriteCategories = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.UserId == userId)
            .Include(oi => oi.Product)
            .GroupBy(oi => oi.Product.CategoryId)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToListAsync();

        // Markaları analiz et
        var favoriteBrands = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.UserId == userId)
            .Include(oi => oi.Product)
            .Where(oi => !string.IsNullOrEmpty(oi.Product.Brand))
            .GroupBy(oi => oi.Product.Brand)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToListAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Kişiselleştirilmiş ürünleri getir
        var query = _context.Products
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
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetPersonalizedRecommendationsAsync(Guid userId, Guid? productId = null, int count = 10)
    {
        if (productId.HasValue)
        {
            // Ürün bazlı öneriler (Frequently Bought Together)
            return await GetProductBasedRecommendationsAsync(productId.Value, count);
        }

        // Kullanıcı bazlı öneriler
        return await GetPersonalizedProductsAsync(userId, count);
    }

    private async Task<IEnumerable<ProductDto>> GetProductBasedRecommendationsAsync(Guid productId, int count)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Bu ürünle birlikte satın alınan ürünleri bul
        var frequentlyBoughtTogether = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.ProductId == productId)
            .SelectMany(oi => oi.Order.OrderItems)
            .Where(oi => oi.ProductId != productId)
            .GroupBy(oi => oi.ProductId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToListAsync();

        if (frequentlyBoughtTogether.Count == 0)
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
            // Eğer birlikte satın alınan ürün yoksa, aynı kategoriden öner
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null)
            {
                frequentlyBoughtTogether = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.CategoryId == product.CategoryId && p.Id != productId && p.IsActive)
                    .OrderByDescending(p => p.Rating)
                    .ThenByDescending(p => p.ReviewCount)
                    .Take(count)
                    .Select(p => p.Id)
                    .ToListAsync();
            }
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => frequentlyBoughtTogether.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<PersonalizationProfileDto> GetUserProfileAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !rv.IsDeleted, !w.IsDeleted, !o.IsDeleted (Global Query Filter)
        var viewedCount = await _context.RecentlyViewedProducts
            .AsNoTracking()
            .CountAsync(rv => rv.UserId == userId);

        var wishlistCount = await _context.Wishlists
            .AsNoTracking()
            .CountAsync(w => w.UserId == userId);

        var orderCount = await _context.Orders
            .AsNoTracking()
            .CountAsync(o => o.UserId == userId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var favoriteCategories = await _context.OrderItems
            .AsNoTracking()
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
            .ToListAsync();

        var favoriteBrands = await _context.OrderItems
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
            .ToListAsync();

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

