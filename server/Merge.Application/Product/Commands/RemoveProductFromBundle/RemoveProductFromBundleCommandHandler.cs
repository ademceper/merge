using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using BundleItem = Merge.Domain.Modules.Catalog.BundleItem;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IBundleRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.ProductBundle>;
using IBundleItemRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.BundleItem>;

namespace Merge.Application.Product.Commands.RemoveProductFromBundle;

public class RemoveProductFromBundleCommandHandler(IBundleRepository bundleRepository, IBundleItemRepository bundleItemRepository, IDbContext context, IUnitOfWork unitOfWork, ICacheService cache, ILogger<RemoveProductFromBundleCommandHandler> logger) : IRequestHandler<RemoveProductFromBundleCommand, bool>
{

    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public async Task<bool> Handle(RemoveProductFromBundleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing product from bundle. BundleId: {BundleId}, ProductId: {ProductId}",
            request.BundleId, request.ProductId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundleItem = await context.Set<BundleItem>()
                .FirstOrDefaultAsync(bi => bi.BundleId == request.BundleId &&
                                     bi.ProductId == request.ProductId, cancellationToken);

            if (bundleItem is null)
            {
                logger.LogWarning("Bundle item not found. BundleId: {BundleId}, ProductId: {ProductId}",
                    request.BundleId, request.ProductId);
                return false;
            }

            var bundle = await bundleRepository.GetByIdAsync(request.BundleId, cancellationToken);
            if (bundle is null)
            {
                logger.LogWarning("Bundle not found. BundleId: {BundleId}", request.BundleId);
                return false;
            }

            bundle.RemoveItem(request.ProductId);

            var newTotal = await context.Set<BundleItem>()
                .AsNoTracking()
                .Include(bi => bi.Product)
                .Where(bi => bi.BundleId == request.BundleId)
                .SumAsync(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity, cancellationToken);

            bundle.UpdateTotalPrices(newTotal);

            await bundleRepository.UpdateAsync(bundle, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_BUNDLE_BY_ID}{request.BundleId}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            logger.LogInformation("Product removed from bundle successfully. BundleId: {BundleId}, ProductId: {ProductId}",
                request.BundleId, request.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing product from bundle. BundleId: {BundleId}, ProductId: {ProductId}",
                request.BundleId, request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
