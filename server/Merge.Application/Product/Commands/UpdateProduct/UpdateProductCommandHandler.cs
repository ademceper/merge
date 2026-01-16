using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<ProductEntity>;

namespace Merge.Application.Product.Commands.UpdateProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IRepository _productRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductCommandHandler> _logger;
    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public UpdateProductCommandHandler(
        IRepository productRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product. ProductId: {ProductId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException("Ürün", request.Id);
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerini güncelleyebilmeli
            if (request.PerformedBy.HasValue && product.SellerId.HasValue && product.SellerId.Value != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to update product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                    request.Id, request.PerformedBy.Value, product.SellerId.Value);
                throw new BusinessException("Bu ürünü güncelleme yetkiniz bulunmamaktadır.");
            }

            // Store old category ID for cache invalidation
            var oldCategoryId = product.CategoryId;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
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

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedProduct = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (reloadedProduct == null)
            {
                _logger.LogWarning("Product {ProductId} not found after update", request.Id);
                throw new NotFoundException("Ürün", request.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            // Note: Paginated cache'ler (products_all_paged_*, products_by_category_*, products_search_*)
            // pattern-based invalidation gerektirir. ICacheService'de RemoveByPrefixAsync yok.
            // Şimdilik cache expiration'a güveniyoruz (15 dakika TTL)
            // Future: Redis SCAN pattern ile prefix-based invalidation eklenebilir
            await _cache.RemoveAsync($"{CACHE_KEY_PRODUCT_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_PRODUCTS_PAGED, cancellationToken);
            if (oldCategoryId != request.CategoryId)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{oldCategoryId}_", cancellationToken);
                await _cache.RemoveAsync($"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{request.CategoryId}_", cancellationToken);
            }
            else
            {
                await _cache.RemoveAsync($"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{request.CategoryId}_", cancellationToken);
            }
            await _cache.RemoveAsync(CACHE_KEY_PRODUCTS_SEARCH, cancellationToken);

            _logger.LogInformation("Product updated successfully. ProductId: {ProductId}", request.Id);

            return _mapper.Map<ProductDto>(reloadedProduct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while updating product Id: {ProductId}", request.Id);
            throw new BusinessException("Ürün güncelleme çakışması. Başka bir kullanıcı aynı ürünü güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product Id: {ProductId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

