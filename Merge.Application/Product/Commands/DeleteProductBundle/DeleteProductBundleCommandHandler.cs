using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.DeleteProductBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteProductBundleCommandHandler : IRequestHandler<DeleteProductBundleCommand, bool>
{
    private readonly IRepository<ProductBundle> _bundleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteProductBundleCommandHandler> _logger;
    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public DeleteProductBundleCommandHandler(
        IRepository<ProductBundle> bundleRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteProductBundleCommandHandler> logger)
    {
        _bundleRepository = bundleRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductBundleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product bundle. BundleId: {BundleId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundle = await _bundleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (bundle == null)
            {
                _logger.LogWarning("Product bundle not found for deletion. BundleId: {BundleId}", request.Id);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            bundle.MarkAsDeleted();
            await _bundleRepository.UpdateAsync(bundle, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_BUNDLE_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            _logger.LogInformation("Product bundle deleted successfully. BundleId: {BundleId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product bundle. BundleId: {BundleId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
