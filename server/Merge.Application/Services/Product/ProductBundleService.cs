using AutoMapper;
using UserEntity = Merge.Domain.Modules.Identity.User;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Product;
using Microsoft.Extensions.Logging;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using ProductBundle = Merge.Domain.Modules.Catalog.ProductBundle;
using BundleItem = Merge.Domain.Modules.Catalog.BundleItem;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IBundleRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.ProductBundle>;
using IBundleItemRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.BundleItem>;
using IProductRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Product>;

namespace Merge.Application.Services.Product;

public class ProductBundleService(IBundleRepository bundleRepository, IBundleItemRepository bundleItemRepository, IProductRepository productRepository, IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductBundleService> logger) : IProductBundleService
{

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductBundleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bundle = await context.Set<ProductBundle>()
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bundle == null) return null;

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Retrieved product bundle. BundleId: {BundleId}", id);
        return MapToDto(bundle);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductBundleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries, removed !b.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        var bundles = await context.Set<ProductBundle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Retrieved all product bundles. Count: {Count}", bundles.Count);
        return bundles.Select(b => MapToDto(b));
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductBundleDto>> GetActiveBundlesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: AsNoTracking for read-only queries, removed !b.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        var bundles = await context.Set<ProductBundle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .Where(b => b.IsActive &&
                  (!b.StartDate.HasValue || b.StartDate.Value <= now) &&
                  (!b.EndDate.HasValue || b.EndDate.Value >= now))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Retrieved active product bundles. Count: {Count}", bundles.Count);
        return bundles.Select(b => MapToDto(b));
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<ProductBundleDto> CreateAsync(CreateProductBundleDto dto, CancellationToken cancellationToken = default)
    {
        if (!dto.Products.Any())
        {
            throw new ValidationException("Paket en az bir ürün içermelidir.");
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product bundle oluşturuluyor. Name: {Name}, ProductCount: {ProductCount}",
            dto.Name, dto.Products.Count);

        // ✅ PERFORMANCE: Fetch all products in a single query to avoid N+1
        // Not: dto.Products zaten memory'de (DTO), bu yüzden ToList() kullanmak gerekiyor
        var productIds = dto.Products.Select(p => p.ProductId).ToList();
        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

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

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var bundle = ProductBundle.Create(
            name: dto.Name,
            description: dto.Description,
            bundlePrice: dto.BundlePrice,
            originalTotalPrice: originalTotal,
            imageUrl: dto.ImageUrl,
            startDate: dto.StartDate,
            endDate: dto.EndDate);

        bundle = await bundleRepository.AddAsync(bundle);

        // Bundle item'ları ekle
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        foreach (var productDto in dto.Products)
        {
            var bundleItem = BundleItem.Create(
                bundleId: bundle.Id,
                productId: productDto.ProductId,
                quantity: productDto.Quantity,
                sortOrder: productDto.SortOrder);
            await bundleItemRepository.AddAsync(bundleItem);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        bundle = await context.Set<ProductBundle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == bundle.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product bundle oluşturuldu. BundleId: {BundleId}, Name: {Name}, BundlePrice: {BundlePrice}",
            bundle!.Id, bundle.Name, bundle.BundlePrice);
        return MapToDto(bundle);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductBundleDto> UpdateAsync(Guid id, UpdateProductBundleDto dto, CancellationToken cancellationToken = default)
    {
        var bundle = await bundleRepository.GetByIdAsync(id);
        if (bundle == null)
        {
            throw new NotFoundException("Paket", id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // ✅ FIX: CS8604 - dto.Name nullable olabilir, null check ekle
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("Paket adı boş olamaz.");
        }
        
        bundle.Update(
            name: dto.Name,
            description: dto.Description,
            bundlePrice: dto.BundlePrice,
            originalTotalPrice: bundle.OriginalTotalPrice,
            imageUrl: dto.ImageUrl,
            startDate: dto.StartDate,
            endDate: dto.EndDate);

        if (dto.IsActive != bundle.IsActive)
        {
            if (dto.IsActive == true)
                bundle.Activate();
            else
                bundle.Deactivate();
        }

        await bundleRepository.UpdateAsync(bundle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var reloadedBundle = await context.Set<ProductBundle>()
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (reloadedBundle == null)
        {
            logger.LogWarning("Product bundle {BundleId} not found after update", id);
            throw new NotFoundException("Product bundle", id);
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Updated product bundle. BundleId: {BundleId}", id);
        return MapToDto(reloadedBundle);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bundle = await bundleRepository.GetByIdAsync(id);
        if (bundle == null)
        {
            return false;
        }

        await bundleRepository.DeleteAsync(bundle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Deleted product bundle. BundleId: {BundleId}", id);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> AddProductToBundleAsync(Guid bundleId, AddProductToBundleDto dto, CancellationToken cancellationToken = default)
    {
        var bundle = await bundleRepository.GetByIdAsync(bundleId);
        if (bundle == null)
        {
            throw new NotFoundException("Paket", bundleId);
        }

        var product = await productRepository.GetByIdAsync(dto.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new NotFoundException("Ürün", dto.ProductId);
        }

        // ✅ PERFORMANCE: Removed !bi.IsDeleted (Global Query Filter)
        var existing = await context.Set<BundleItem>()
            .AsNoTracking()
            .FirstOrDefaultAsync(bi => bi.BundleId == bundleId &&
                                 bi.ProductId == dto.ProductId, cancellationToken);

        if (existing != null)
        {
            throw new BusinessException("Bu ürün zaten pakette.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var bundleItem = BundleItem.Create(
            bundleId: bundleId,
            productId: dto.ProductId,
            quantity: dto.Quantity,
            sortOrder: dto.SortOrder);

        await bundleItemRepository.AddAsync(bundleItem);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var newTotal = await context.Set<BundleItem>()
            .AsNoTracking()
            .Include(bi => bi.Product)
            .Where(bi => bi.BundleId == bundleId)
            .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        bundle.UpdateTotalPrices(newTotal);
        await bundleRepository.UpdateAsync(bundle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Added product to bundle. BundleId: {BundleId}, ProductId: {ProductId}", bundleId, dto.ProductId);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RemoveProductFromBundleAsync(Guid bundleId, Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed !bi.IsDeleted (Global Query Filter)
        var bundleItem = await context.Set<BundleItem>()
            .FirstOrDefaultAsync(bi => bi.BundleId == bundleId &&
                                 bi.ProductId == productId, cancellationToken);

        if (bundleItem == null)
        {
            return false;
        }

        await bundleItemRepository.DeleteAsync(bundleItem);

        // Orijinal toplam fiyatı güncelle
        var bundle = await bundleRepository.GetByIdAsync(bundleId);
        if (bundle != null)
        {
            // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
            var newTotal = await context.Set<BundleItem>()
                .AsNoTracking()
                .Include(bi => bi.Product)
                .Where(bi => bi.BundleId == bundleId)
                .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            bundle.UpdateTotalPrices(newTotal);
            await bundleRepository.UpdateAsync(bundle);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Removed product from bundle. BundleId: {BundleId}, ProductId: {ProductId}", bundleId, productId);
        return true;
    }

    private ProductBundleDto MapToDto(ProductBundle bundle)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: Items AutoMapper'da zaten map ediliyor (OrderBy ile SortOrder)
        return mapper.Map<ProductBundleDto>(bundle);
    }
}

