using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using BundleItem = Merge.Domain.Modules.Catalog.BundleItem;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IBundleRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.ProductBundle>;
using IBundleItemRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.BundleItem>;
using IProductRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Product>;

namespace Merge.Application.Product.Commands.AddProductToBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AddProductToBundleCommandHandler(IBundleRepository bundleRepository, IBundleItemRepository bundleItemRepository, IProductRepository productRepository, IDbContext context, IUnitOfWork unitOfWork, ICacheService cache, ILogger<AddProductToBundleCommandHandler> logger) : IRequestHandler<AddProductToBundleCommand, bool>
{

    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public async Task<bool> Handle(AddProductToBundleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding product to bundle. BundleId: {BundleId}, ProductId: {ProductId}",
            request.BundleId, request.ProductId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundle = await bundleRepository.GetByIdAsync(request.BundleId, cancellationToken);
            if (bundle == null)
            {
                throw new NotFoundException("Paket", request.BundleId);
            }

            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null || !product.IsActive)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // Note: Duplicate check zaten ProductBundle.AddItem() method'unda yapılıyor
            bundle.AddItem(request.ProductId, request.Quantity, request.SortOrder);

            // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
            var newTotal = await context.Set<BundleItem>()
                .AsNoTracking()
                .Include(bi => bi.Product)
                .Where(bi => bi.BundleId == request.BundleId)
                .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            bundle.UpdateTotalPrices(newTotal);

            await bundleRepository.UpdateAsync(bundle, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await cache.RemoveAsync($"{CACHE_KEY_BUNDLE_BY_ID}{request.BundleId}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            logger.LogInformation("Product added to bundle successfully. BundleId: {BundleId}, ProductId: {ProductId}",
                request.BundleId, request.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding product to bundle. BundleId: {BundleId}, ProductId: {ProductId}",
                request.BundleId, request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
