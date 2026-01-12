using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using OrderItemEntity = Merge.Domain.Modules.Ordering.OrderItem;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.DeleteProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<ProductEntity> _productRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteProductCommandHandler> _logger;
    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public DeleteProductCommandHandler(
        Merge.Application.Interfaces.IRepository<ProductEntity> productRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product. ProductId: {ProductId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product not found for deletion. ProductId: {ProductId}", request.Id);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerini silebilmeli
            if (request.PerformedBy.HasValue && product.SellerId.HasValue && product.SellerId.Value != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to delete product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                    request.Id, request.PerformedBy.Value, product.SellerId.Value);
                throw new BusinessException("Bu ürünü silme yetkiniz bulunmamaktadır.");
            }

            // Store category ID for cache invalidation
            var categoryId = product.CategoryId;

            // Check for associated orders
            var hasOrders = await _context.Set<OrderItemEntity>()
                .AsNoTracking()
                .AnyAsync(oi => oi.ProductId == request.Id, cancellationToken);

            if (hasOrders)
            {
                throw new BusinessException("Siparişleri olan bir ürün silinemez.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            product.MarkAsDeleted();
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Product deleted successfully. ProductId: {ProductId}", request.Id);

            // ✅ BOLUM 10.2: Cache invalidation
            // Note: Paginated cache'ler (products_all_paged_*, products_by_category_*, products_search_*)
            // pattern-based invalidation gerektirir. ICacheService'de RemoveByPrefixAsync yok.
            // Şimdilik cache expiration'a güveniyoruz (15 dakika TTL)
            // Future: Redis SCAN pattern ile prefix-based invalidation eklenebilir
            await _cache.RemoveAsync($"{CACHE_KEY_PRODUCT_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_PRODUCTS_PAGED, cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{categoryId}_", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_PRODUCTS_SEARCH, cancellationToken);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting product Id: {ProductId}", request.Id);
            throw new BusinessException("Ürün silme çakışması. Başka bir kullanıcı aynı ürünü güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product Id: {ProductId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

