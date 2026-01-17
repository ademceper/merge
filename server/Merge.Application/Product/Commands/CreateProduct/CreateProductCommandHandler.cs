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
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Product>;

namespace Merge.Application.Product.Commands.CreateProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateProductCommandHandler(IRepository productRepository, IDbContext context, IUnitOfWork unitOfWork, ICacheService cache, IMapper mapper, ILogger<CreateProductCommandHandler> logger) : IRequestHandler<CreateProductCommand, ProductDto>
{

    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product. Name: {Name}, SKU: {SKU}, SellerId: {SellerId}",
            request.Name, request.SKU, request.SellerId);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var sku = new SKU(request.SKU);
        var price = new Money(request.Price);
        var product = ProductEntity.Create(
            request.Name,
            request.Description,
            sku,
            price,
            request.StockQuantity,
            request.CategoryId,
            request.Brand,
            request.SellerId,
            request.StoreId);

        // Set discount price if provided
        if (request.DiscountPrice.HasValue)
        {
            product.SetDiscountPrice(new Money(request.DiscountPrice.Value));
        }

        // Set images
        product.UpdateImages(request.ImageUrl, request.ImageUrls ?? new List<string>());

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await productRepository.AddAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedProduct = await context.Set<ProductEntity>()
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

            if (reloadedProduct == null)
            {
                logger.LogWarning("Product {ProductId} not found after creation", product.Id);
                throw new NotFoundException("Ürün", product.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            // Note: Paginated cache'ler (products_all_paged_*, products_by_category_*, products_search_*)
            // pattern-based invalidation gerektirir. ICacheService'de RemoveByPrefixAsync yok.
            // Şimdilik cache expiration'a güveniyoruz (15 dakika TTL)
            // Future: Redis SCAN pattern ile prefix-based invalidation eklenebilir
            await cache.RemoveAsync($"{CACHE_KEY_PRODUCT_BY_ID}{product.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_PRODUCTS_PAGED, cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{request.CategoryId}_", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_PRODUCTS_SEARCH, cancellationToken);

            logger.LogInformation("Product created successfully. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}",
                product.Id, request.Name, request.SKU);

            return mapper.Map<ProductDto>(reloadedProduct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product. Name: {Name}, SKU: {SKU}", request.Name, request.SKU);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

