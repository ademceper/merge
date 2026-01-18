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

    public async Task<ProductBundleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bundle = await context.Set<ProductBundle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bundle is null) return null;

        logger.LogInformation("Retrieved product bundle. BundleId: {BundleId}", id);
        return MapToDto(bundle);
    }

    public async Task<IEnumerable<ProductBundleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bundles = await context.Set<ProductBundle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved all product bundles. Count: {Count}", bundles.Count);
        return bundles.Select(b => MapToDto(b));
    }

    public async Task<IEnumerable<ProductBundleDto>> GetActiveBundlesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
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

        logger.LogInformation("Retrieved active product bundles. Count: {Count}", bundles.Count);
        return bundles.Select(b => MapToDto(b));
    }

    public async Task<ProductBundleDto> CreateAsync(CreateProductBundleDto dto, CancellationToken cancellationToken = default)
    {
        if (!dto.Products.Any())
        {
            throw new ValidationException("Paket en az bir ürün içermelidir.");
        }

        logger.LogInformation(
            "Product bundle oluşturuluyor. Name: {Name}, ProductCount: {ProductCount}",
            dto.Name, dto.Products.Count);

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

        bundle = await context.Set<ProductBundle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == bundle.Id, cancellationToken);

        logger.LogInformation(
            "Product bundle oluşturuldu. BundleId: {BundleId}, Name: {Name}, BundlePrice: {BundlePrice}",
            bundle!.Id, bundle.Name, bundle.BundlePrice);
        return MapToDto(bundle);
    }

    public async Task<ProductBundleDto> UpdateAsync(Guid id, UpdateProductBundleDto dto, CancellationToken cancellationToken = default)
    {
        var bundle = await bundleRepository.GetByIdAsync(id);
        if (bundle is null)
        {
            throw new NotFoundException("Paket", id);
        }

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
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (reloadedBundle is null)
        {
            logger.LogWarning("Product bundle {BundleId} not found after update", id);
            throw new NotFoundException("Product bundle", id);
        }

        logger.LogInformation("Updated product bundle. BundleId: {BundleId}", id);
        return MapToDto(reloadedBundle);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bundle = await bundleRepository.GetByIdAsync(id);
        if (bundle is null)
        {
            return false;
        }

        await bundleRepository.DeleteAsync(bundle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted product bundle. BundleId: {BundleId}", id);
        return true;
    }

    public async Task<bool> AddProductToBundleAsync(Guid bundleId, AddProductToBundleDto dto, CancellationToken cancellationToken = default)
    {
        var bundle = await bundleRepository.GetByIdAsync(bundleId);
        if (bundle is null)
        {
            throw new NotFoundException("Paket", bundleId);
        }

        var product = await productRepository.GetByIdAsync(dto.ProductId);
        if (product is null || !product.IsActive)
        {
            throw new NotFoundException("Ürün", dto.ProductId);
        }

        var existing = await context.Set<BundleItem>()
            .AsNoTracking()
            .FirstOrDefaultAsync(bi => bi.BundleId == bundleId &&
                                 bi.ProductId == dto.ProductId, cancellationToken);

        if (existing is not null)
        {
            throw new BusinessException("Bu ürün zaten pakette.");
        }

        var bundleItem = BundleItem.Create(
            bundleId: bundleId,
            productId: dto.ProductId,
            quantity: dto.Quantity,
            sortOrder: dto.SortOrder);

        await bundleItemRepository.AddAsync(bundleItem);

        var newTotal = await context.Set<BundleItem>()
            .AsNoTracking()
            .Include(bi => bi.Product)
            .Where(bi => bi.BundleId == bundleId)
            .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity, cancellationToken);

        bundle.UpdateTotalPrices(newTotal);
        await bundleRepository.UpdateAsync(bundle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added product to bundle. BundleId: {BundleId}, ProductId: {ProductId}", bundleId, dto.ProductId);
        return true;
    }

    public async Task<bool> RemoveProductFromBundleAsync(Guid bundleId, Guid productId, CancellationToken cancellationToken = default)
    {
        var bundleItem = await context.Set<BundleItem>()
            .FirstOrDefaultAsync(bi => bi.BundleId == bundleId &&
                                 bi.ProductId == productId, cancellationToken);

        if (bundleItem is null)
        {
            return false;
        }

        await bundleItemRepository.DeleteAsync(bundleItem);

        // Orijinal toplam fiyatı güncelle
        var bundle = await bundleRepository.GetByIdAsync(bundleId);
        if (bundle is not null)
        {
            var newTotal = await context.Set<BundleItem>()
                .AsNoTracking()
                .Include(bi => bi.Product)
                .Where(bi => bi.BundleId == bundleId)
                .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity, cancellationToken);

            bundle.UpdateTotalPrices(newTotal);
            await bundleRepository.UpdateAsync(bundle);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Removed product from bundle. BundleId: {BundleId}, ProductId: {ProductId}", bundleId, productId);
        return true;
    }

    private ProductBundleDto MapToDto(ProductBundle bundle)
    {
        // Not: Items AutoMapper'da zaten map ediliyor (OrderBy ile SortOrder)
        return mapper.Map<ProductBundleDto>(bundle);
    }
}

