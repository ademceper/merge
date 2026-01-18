using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Product>;

namespace Merge.Application.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IRepository productRepository, IDbContext context, IUnitOfWork unitOfWork, ICacheService cache, IMapper mapper, ILogger<UpdateProductCommandHandler> logger) : IRequestHandler<UpdateProductCommand, ProductDto>
{

    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating product. ProductId: {ProductId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
            {
                throw new NotFoundException("Ürün", request.Id);
            }

            if (request.PerformedBy.HasValue && product.SellerId.HasValue && product.SellerId.Value != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to update product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                    request.Id, request.PerformedBy.Value, product.SellerId.Value);
                throw new BusinessException("Bu ürünü güncelleme yetkiniz bulunmamaktadır.");
            }

            // Store old category ID for cache invalidation
            var oldCategoryId = product.CategoryId;

            product.UpdateName(request.Name);
            product.UpdateDescription(request.Description);
            
            var sku = new SKU(request.SKU);
            product.UpdateSKU(sku);
            
            var price = new Money(request.Price);
            product.SetPrice(price);
            
            if (request.DiscountPrice.HasValue)
            {
                var discountPrice = new Money(request.DiscountPrice.Value);
                product.SetDiscountPrice(discountPrice);
            }
            else
            {
                product.SetDiscountPrice(null);
            }
            
            product.SetStockQuantity(request.StockQuantity);
            product.UpdateBrand(request.Brand);
            product.UpdateImages(request.ImageUrl, request.ImageUrls ?? new List<string>());
            
            if (request.IsActive)
                product.Activate();
            else
                product.Deactivate();
            
            product.SetCategory(request.CategoryId);

            await productRepository.UpdateAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedProduct = await context.Set<ProductEntity>()
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (reloadedProduct is null)
            {
                logger.LogWarning("Product {ProductId} not found after update", request.Id);
                throw new NotFoundException("Ürün", request.Id);
            }

            // Note: Paginated cache'ler (products_all_paged_*, products_by_category_*, products_search_*)
            // pattern-based invalidation gerektirir. ICacheService'de RemoveByPrefixAsync yok.
            // Şimdilik cache expiration'a güveniyoruz (15 dakika TTL)
            // Future: Redis SCAN pattern ile prefix-based invalidation eklenebilir
            await cache.RemoveAsync($"{CACHE_KEY_PRODUCT_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_PRODUCTS_PAGED, cancellationToken);
            if (oldCategoryId != request.CategoryId)
            {
                await cache.RemoveAsync($"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{oldCategoryId}_", cancellationToken);
                await cache.RemoveAsync($"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{request.CategoryId}_", cancellationToken);
            }
            else
            {
                await cache.RemoveAsync($"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{request.CategoryId}_", cancellationToken);
            }
            await cache.RemoveAsync(CACHE_KEY_PRODUCTS_SEARCH, cancellationToken);

            logger.LogInformation("Product updated successfully. ProductId: {ProductId}", request.Id);

            return mapper.Map<ProductDto>(reloadedProduct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating product Id: {ProductId}", request.Id);
            throw new BusinessException("Ürün güncelleme çakışması. Başka bir kullanıcı aynı ürünü güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product Id: {ProductId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

