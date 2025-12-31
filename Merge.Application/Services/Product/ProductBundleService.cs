using AutoMapper;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Product;
using Microsoft.Extensions.Logging;


namespace Merge.Application.Services.Product;

public class ProductBundleService : IProductBundleService
{
    private readonly IRepository<ProductBundle> _bundleRepository;
    private readonly IRepository<BundleItem> _bundleItemRepository;
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductBundleService> _logger;

    public ProductBundleService(
        IRepository<ProductBundle> bundleRepository,
        IRepository<BundleItem> bundleItemRepository,
        IRepository<ProductEntity> productRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductBundleService> logger)
    {
        _bundleRepository = bundleRepository;
        _bundleItemRepository = bundleItemRepository;
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductBundleDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries, removed !b.IsDeleted (Global Query Filter)
        var bundle = await _context.ProductBundles
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bundle == null) return null;

        _logger.LogInformation("Retrieved product bundle. BundleId: {BundleId}", id);
        return MapToDto(bundle);
    }

    public async Task<IEnumerable<ProductBundleDto>> GetAllAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries, removed !b.IsDeleted (Global Query Filter)
        var bundles = await _context.ProductBundles
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved all product bundles. Count: {Count}", bundles.Count);
        return bundles.Select(b => MapToDto(b));
    }

    public async Task<IEnumerable<ProductBundleDto>> GetActiveBundlesAsync()
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: AsNoTracking for read-only queries, removed !b.IsDeleted (Global Query Filter)
        var bundles = await _context.ProductBundles
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .Where(b => b.IsActive &&
                  (!b.StartDate.HasValue || b.StartDate.Value <= now) &&
                  (!b.EndDate.HasValue || b.EndDate.Value >= now))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved active product bundles. Count: {Count}", bundles.Count);
        return bundles.Select(b => MapToDto(b));
    }

    public async Task<ProductBundleDto> CreateAsync(CreateProductBundleDto dto)
    {
        if (!dto.Products.Any())
        {
            throw new ValidationException("Paket en az bir ürün içermelidir.");
        }

        // ✅ PERFORMANCE: Fetch all products in a single query to avoid N+1
        var productIds = dto.Products.Select(p => p.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id);

        // Validate all products exist
        foreach (var productDto in dto.Products)
        {
            if (!products.ContainsKey(productDto.ProductId))
            {
                throw new NotFoundException("Ürün", productDto.ProductId);
            }
        }

        // Calculate original total price
        decimal originalTotal = dto.Products.Sum(productDto =>
            (products[productDto.ProductId].DiscountPrice ?? products[productDto.ProductId].Price) * productDto.Quantity);

        var discountPercentage = ((originalTotal - dto.BundlePrice) / originalTotal) * 100;

        var bundle = new ProductBundle
        {
            Name = dto.Name,
            Description = dto.Description,
            BundlePrice = dto.BundlePrice,
            OriginalTotalPrice = originalTotal,
            DiscountPercentage = discountPercentage,
            ImageUrl = dto.ImageUrl,
            IsActive = true,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        bundle = await _bundleRepository.AddAsync(bundle);

        // Bundle item'ları ekle
        foreach (var productDto in dto.Products)
        {
            var bundleItem = new BundleItem
            {
                BundleId = bundle.Id,
                ProductId = productDto.ProductId,
                Quantity = productDto.Quantity,
                SortOrder = productDto.SortOrder
            };
            await _bundleItemRepository.AddAsync(bundleItem);
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        bundle = await _context.ProductBundles
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == bundle.Id);

        _logger.LogInformation("Created product bundle. BundleId: {BundleId}, Name: {Name}", bundle.Id, bundle.Name);
        return MapToDto(bundle);
    }

    public async Task<ProductBundleDto> UpdateAsync(Guid id, UpdateProductBundleDto dto)
    {
        var bundle = await _bundleRepository.GetByIdAsync(id);
        if (bundle == null)
        {
            throw new NotFoundException("Paket", id);
        }

        bundle.Name = dto.Name;
        bundle.Description = dto.Description;
        bundle.BundlePrice = dto.BundlePrice;
        bundle.ImageUrl = dto.ImageUrl;
        bundle.IsActive = dto.IsActive;
        bundle.StartDate = dto.StartDate;
        bundle.EndDate = dto.EndDate;

        // İndirim yüzdesini yeniden hesapla
        if (bundle.OriginalTotalPrice.HasValue)
        {
            bundle.DiscountPercentage = ((bundle.OriginalTotalPrice.Value - dto.BundlePrice) / bundle.OriginalTotalPrice.Value) * 100;
        }

        await _bundleRepository.UpdateAsync(bundle);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        bundle = await _context.ProductBundles
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == id);

        _logger.LogInformation("Updated product bundle. BundleId: {BundleId}", id);
        return MapToDto(bundle);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var bundle = await _bundleRepository.GetByIdAsync(id);
        if (bundle == null)
        {
            return false;
        }

        await _bundleRepository.DeleteAsync(bundle);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted product bundle. BundleId: {BundleId}", id);
        return true;
    }

    public async Task<bool> AddProductToBundleAsync(Guid bundleId, AddProductToBundleDto dto)
    {
        var bundle = await _bundleRepository.GetByIdAsync(bundleId);
        if (bundle == null)
        {
            throw new NotFoundException("Paket", bundleId);
        }

        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new NotFoundException("Ürün", dto.ProductId);
        }

        // ✅ PERFORMANCE: Removed !bi.IsDeleted (Global Query Filter)
        var existing = await _context.BundleItems
            .AsNoTracking()
            .FirstOrDefaultAsync(bi => bi.BundleId == bundleId &&
                                 bi.ProductId == dto.ProductId);

        if (existing != null)
        {
            throw new BusinessException("Bu ürün zaten pakette.");
        }

        var bundleItem = new BundleItem
        {
            BundleId = bundleId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            SortOrder = dto.SortOrder
        };

        await _bundleItemRepository.AddAsync(bundleItem);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var newTotal = await _context.BundleItems
            .AsNoTracking()
            .Include(bi => bi.Product)
            .Where(bi => bi.BundleId == bundleId)
            .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity);

        bundle.OriginalTotalPrice = newTotal;
        bundle.DiscountPercentage = ((newTotal - bundle.BundlePrice) / newTotal) * 100;
        await _bundleRepository.UpdateAsync(bundle);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Added product to bundle. BundleId: {BundleId}, ProductId: {ProductId}", bundleId, dto.ProductId);
        return true;
    }

    public async Task<bool> RemoveProductFromBundleAsync(Guid bundleId, Guid productId)
    {
        // ✅ PERFORMANCE: Removed !bi.IsDeleted (Global Query Filter)
        var bundleItem = await _context.BundleItems
            .FirstOrDefaultAsync(bi => bi.BundleId == bundleId &&
                                 bi.ProductId == productId);

        if (bundleItem == null)
        {
            return false;
        }

        await _bundleItemRepository.DeleteAsync(bundleItem);

        // Orijinal toplam fiyatı güncelle
        var bundle = await _bundleRepository.GetByIdAsync(bundleId);
        if (bundle != null)
        {
            // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
            var newTotal = await _context.BundleItems
                .AsNoTracking()
                .Include(bi => bi.Product)
                .Where(bi => bi.BundleId == bundleId)
                .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity);

            bundle.OriginalTotalPrice = newTotal;
            if (newTotal > 0)
            {
                bundle.DiscountPercentage = ((newTotal - bundle.BundlePrice) / newTotal) * 100;
            }
            await _bundleRepository.UpdateAsync(bundle);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Removed product from bundle. BundleId: {BundleId}, ProductId: {ProductId}", bundleId, productId);
        return true;
    }

    private ProductBundleDto MapToDto(ProductBundle bundle)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: Items AutoMapper'da zaten map ediliyor (OrderBy ile SortOrder)
        return _mapper.Map<ProductBundleDto>(bundle);
    }
}

