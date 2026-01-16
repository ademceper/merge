using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IBundleRepository = Merge.Application.Interfaces.IRepository<ProductBundle>;
using IBundleItemRepository = Merge.Application.Interfaces.IRepository<BundleItem>;

namespace Merge.Application.Product.Commands.RemoveProductFromBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class RemoveProductFromBundleCommandHandler : IRequestHandler<RemoveProductFromBundleCommand, bool>
{
    private readonly IBundleRepository _bundleRepository;
    private readonly IBundleItemRepository _bundleItemRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<RemoveProductFromBundleCommandHandler> _logger;
    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public RemoveProductFromBundleCommandHandler(
        IBundleRepository bundleRepository,
        IBundleItemRepository bundleItemRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<RemoveProductFromBundleCommandHandler> logger)
    {
        _bundleRepository = bundleRepository;
        _bundleItemRepository = bundleItemRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveProductFromBundleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing product from bundle. BundleId: {BundleId}, ProductId: {ProductId}",
            request.BundleId, request.ProductId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ PERFORMANCE: Removed !bi.IsDeleted (Global Query Filter)
            var bundleItem = await _context.Set<BundleItem>()
                .FirstOrDefaultAsync(bi => bi.BundleId == request.BundleId &&
                                     bi.ProductId == request.ProductId, cancellationToken);

            if (bundleItem == null)
            {
                _logger.LogWarning("Bundle item not found. BundleId: {BundleId}, ProductId: {ProductId}",
                    request.BundleId, request.ProductId);
                return false;
            }

            var bundle = await _bundleRepository.GetByIdAsync(request.BundleId, cancellationToken);
            if (bundle == null)
            {
                _logger.LogWarning("Bundle not found. BundleId: {BundleId}", request.BundleId);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            bundle.RemoveItem(request.ProductId);

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

            _logger.LogInformation("Product removed from bundle successfully. BundleId: {BundleId}, ProductId: {ProductId}",
                request.BundleId, request.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product from bundle. BundleId: {BundleId}, ProductId: {ProductId}",
                request.BundleId, request.ProductId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
