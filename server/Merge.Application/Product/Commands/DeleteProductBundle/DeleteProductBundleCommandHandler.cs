using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using ProductBundle = Merge.Domain.Modules.Catalog.ProductBundle;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.ProductBundle>;

namespace Merge.Application.Product.Commands.DeleteProductBundle;

public class DeleteProductBundleCommandHandler(IRepository bundleRepository, IUnitOfWork unitOfWork, ICacheService cache, ILogger<DeleteProductBundleCommandHandler> logger) : IRequestHandler<DeleteProductBundleCommand, bool>
{

    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public async Task<bool> Handle(DeleteProductBundleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting product bundle. BundleId: {BundleId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundle = await bundleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (bundle == null)
            {
                logger.LogWarning("Product bundle not found for deletion. BundleId: {BundleId}", request.Id);
                return false;
            }

            bundle.MarkAsDeleted();
            await bundleRepository.UpdateAsync(bundle, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_BUNDLE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            logger.LogInformation("Product bundle deleted successfully. BundleId: {BundleId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product bundle. BundleId: {BundleId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
