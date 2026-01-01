using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Entities.User;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using ProductEntity = Merge.Domain.Entities.Product;
using ReviewEntity = Merge.Domain.Entities.Review;
using Merge.Application.DTOs.Product;


namespace Merge.Application.Services.Product;

public class ProductComparisonService : IProductComparisonService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductComparisonService> _logger;

    public ProductComparisonService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductComparisonService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductComparisonDto> CreateComparisonAsync(Guid userId, CreateComparisonDto dto)
    {
        if (dto.ProductIds.Count > 5)
        {
            throw new ValidationException("Aynı anda en fazla 5 ürün karşılaştırılabilir.");
        }

        var comparison = new ProductComparison
        {
            UserId = userId,
            Name = dto.Name ?? "Unnamed Comparison",
            IsSaved = !string.IsNullOrEmpty(dto.Name)
        };

        await _context.Set<ProductComparison>().AddAsync(comparison);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = dto.ProductIds.Distinct().ToList();
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        int position = 0;
        foreach (var productId in dto.ProductIds)
        {
            if (products.ContainsKey(productId))
            {
                var item = new ProductComparisonItem
                {
                    ComparisonId = comparison.Id,
                    ProductId = productId,
                    Position = position++
                };

                await _context.Set<ProductComparisonItem>().AddAsync(item);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        comparison = await _context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparison.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return await MapToDto(comparison);
    }

    public async Task<ProductComparisonDto?> GetComparisonAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return comparison != null ? await MapToDto(comparison) : null;
    }

    public async Task<ProductComparisonDto?> GetUserComparisonAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (comparison == null)
        {
            // Create a new temporary comparison
            comparison = new ProductComparison
            {
                UserId = userId,
                Name = "Current Comparison",
                IsSaved = false
            };

            await _context.Set<ProductComparison>().AddAsync(comparison);
            await _unitOfWork.SaveChangesAsync();
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return await MapToDto(comparison);
    }

    public async Task<IEnumerable<ProductComparisonDto>> GetUserComparisonsAsync(Guid userId, bool savedOnly = false)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var query = _context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Where(c => c.UserId == userId);

        if (savedOnly)
        {
            query = query.Where(c => c.IsSaved);
        }

        var comparisons = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: MapToDto metodu hala kullanılıyor çünkü Products listesi manuel set ediliyor
        var dtos = new List<ProductComparisonDto>();
        foreach (var comparison in comparisons)
        {
            dtos.Add(await MapToDto(comparison));
        }

        return dtos;
    }

    public async Task<ProductComparisonDto?> GetComparisonByShareCodeAsync(string shareCode)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.ShareCode == shareCode);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return comparison != null ? await MapToDto(comparison) : null;
    }

    public async Task<ProductComparisonDto> AddProductToComparisonAsync(Guid userId, Guid productId)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (comparison == null)
        {
            comparison = new ProductComparison
            {
                UserId = userId,
                Name = "Current Comparison",
                IsSaved = false
            };

            await _context.Set<ProductComparison>().AddAsync(comparison);
            await _unitOfWork.SaveChangesAsync();
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
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        var item = new ProductComparisonItem
        {
            ComparisonId = comparison.Id,
            ProductId = productId,
            Position = comparison.Items.Count
        };

        await _context.Set<ProductComparisonItem>().AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        comparison = await _context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparison.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return await MapToDto(comparison!);
    }

    public async Task<bool> RemoveProductFromComparisonAsync(Guid userId, Guid productId)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (comparison == null) return false;

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var item = comparison.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return false;

        item.IsDeleted = true;

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        // Reorder remaining items
        var remainingItems = comparison.Items
            .OrderBy(i => i.Position)
            .ToList();

        for (int i = 0; i < remainingItems.Count; i++)
        {
            remainingItems[i].Position = i;
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SaveComparisonAsync(Guid userId, string name)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (comparison == null) return false;

        comparison.Name = name;
        comparison.IsSaved = true;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string> GenerateShareCodeAsync(Guid comparisonId)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .FirstOrDefaultAsync(c => c.Id == comparisonId);

        if (comparison == null)
        {
            throw new NotFoundException("Karşılaştırma", comparisonId);
        }

        if (!string.IsNullOrEmpty(comparison.ShareCode))
        {
            return comparison.ShareCode;
        }

        var shareCode = GenerateUniqueShareCode();
        comparison.ShareCode = shareCode;

        await _unitOfWork.SaveChangesAsync();

        return shareCode;
    }

    public async Task<bool> ClearComparisonAsync(Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .Include(c => c.Items)
            .Where(c => c.UserId == userId && !c.IsSaved)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (comparison == null) return false;

        foreach (var item in comparison.Items)
        {
            item.IsDeleted = true;
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteComparisonAsync(Guid id, Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (comparison == null) return false;

        comparison.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<ComparisonMatrixDto> GetComparisonMatrixAsync(Guid comparisonId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var comparison = await _context.Set<ProductComparison>()
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == comparisonId);

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
        var reviewsDict = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => productIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Rating = g.Average(r => r.Rating),
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.ProductId);

        var matrix = new ComparisonMatrixDto
        {
            AttributeNames = new List<string>
            {
                "Price",
                "Stock",
                "Rating",
                "Reviews",
                "Brand",
                "Category"
            }
        };

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
            var compProduct = _mapper.Map<ComparisonProductDto>(product);
            compProduct.Rating = reviewStats != null ? (decimal?)reviewStats.Rating : null;
            compProduct.ReviewCount = reviewStats?.Count ?? 0;
            compProduct.Specifications = new Dictionary<string, string>(); // TODO: Map from product specifications
            compProduct.Features = new List<string>(); // TODO: Map from product features
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
            if (!matrix.AttributeNames.Contains(key))
            {
                matrix.AttributeNames.Add(key);
                attributeValues[key] = comparisonProducts
                    .Select(p => p.Specifications.ContainsKey(key) ? p.Specifications[key] : "N/A")
                    .ToList();
            }
        }

        matrix.Products = comparisonProducts;
        matrix.AttributeValues = attributeValues;

        return matrix;
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison)
    {
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var items = await _context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load reviews to avoid N+1 queries
        var productIds = items.Select(i => i.ProductId).ToList();
        var reviewsDict = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => productIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Rating = g.Average(r => r.Rating),
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.ProductId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: ComparisonProductDto için AutoMapper mapping'i eklenmeli
        var products = new List<ComparisonProductDto>();

        foreach (var item in items)
        {
            var reviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats) ? stats : null;

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // Not: ProductComparisonItem → ComparisonProductDto mapping'i eklenmeli
            var compProduct = _mapper.Map<ComparisonProductDto>(item.Product);
            compProduct.Position = item.Position;
            compProduct.Rating = reviewStats != null ? (decimal?)reviewStats.Rating : null;
            compProduct.ReviewCount = reviewStats?.Count ?? 0;
            compProduct.Specifications = new Dictionary<string, string>(); // TODO: Map from product specifications
            compProduct.Features = new List<string>(); // TODO: Map from product features
            products.Add(compProduct);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var comparisonDto = _mapper.Map<ProductComparisonDto>(comparison);
        comparisonDto.Products = products;
        return comparisonDto;
    }

    private string GenerateUniqueShareCode()
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
