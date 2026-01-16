using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IBundleRepository = Merge.Application.Interfaces.IRepository<ProductBundle>;
using IBundleItemRepository = Merge.Application.Interfaces.IRepository<BundleItem>;
using IProductRepository = Merge.Application.Interfaces.IRepository<ProductEntity>;

namespace Merge.Application.Product.Commands.AddProductToBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AddProductToBundleCommandHandler : IRequestHandler<AddProductToBundleCommand, bool>
{
    private readonly IBundleRepository _bundleRepository;
    private readonly IBundleItemRepository _bundleItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<AddProductToBundleCommandHandler> _logger;
    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public AddProductToBundleCommandHandler(
        IBundleRepository bundleRepository,
        IBundleItemRepository bundleItemRepository,
        IProductRepository productRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<AddProductToBundleCommandHandler> logger)
    {
        _bundleRepository = bundleRepository;
        _bundleItemRepository = bundleItemRepository;
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(AddProductToBundleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding product to bundle. BundleId: {BundleId}, ProductId: {ProductId}",
            request.BundleId, request.ProductId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundle = await _bundleRepository.GetByIdAsync(request.BundleId, cancellationToken);
            if (bundle == null)
            {
                throw new NotFoundException("Paket", request.BundleId);
            }

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null || !product.IsActive)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // Note: Duplicate check zaten ProductBundle.AddItem() method'unda yapılıyor
            bundle.AddItem(request.ProductId, request.Quantity, request.SortOrder);

            // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
            var newTotal = await _context.Set<BundleItem>()
                .AsNoTracking()
                .Include(bi => bi.Product)
                .Where(bi => bi.BundleId == request.BundleId)
                .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            bundle.UpdateTotalPrices(newTotal);

            await _bundleRepository.UpdateAsync(bundle, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_BUNDLE_BY_ID}{request.BundleId}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            _logger.LogInformation("Product added to bundle successfully. BundleId: {BundleId}, ProductId: {ProductId}",
                request.BundleId, request.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to bundle. BundleId: {BundleId}, ProductId: {ProductId}",
                request.BundleId, request.ProductId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
