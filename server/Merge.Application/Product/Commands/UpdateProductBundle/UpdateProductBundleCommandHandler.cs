using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using ProductBundle = Merge.Domain.Modules.Catalog.ProductBundle;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.ProductBundle>;

namespace Merge.Application.Product.Commands.UpdateProductBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateProductBundleCommandHandler(IRepository bundleRepository, IDbContext context, IUnitOfWork unitOfWork, ICacheService cache, IMapper mapper, ILogger<UpdateProductBundleCommandHandler> logger) : IRequestHandler<UpdateProductBundleCommand, ProductBundleDto>
{

    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public async Task<ProductBundleDto> Handle(UpdateProductBundleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating product bundle. BundleId: {BundleId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundle = await bundleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (bundle == null)
            {
                throw new NotFoundException("Paket", request.Id);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            bundle.Update(
                request.Name,
                request.Description,
                request.BundlePrice,
                bundle.OriginalTotalPrice,
                request.ImageUrl,
                request.StartDate,
                request.EndDate);

            if (request.IsActive)
            {
                bundle.Activate();
            }
            else
            {
                bundle.Deactivate();
            }

            await bundleRepository.UpdateAsync(bundle, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedBundle = await context.Set<ProductBundle>()
                .AsNoTracking()
                .Include(b => b.BundleItems)
                    .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (reloadedBundle == null)
            {
                logger.LogWarning("Product bundle {BundleId} not found after update", request.Id);
                throw new NotFoundException("Paket", request.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await cache.RemoveAsync($"{CACHE_KEY_BUNDLE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            logger.LogInformation("Product bundle updated successfully. BundleId: {BundleId}", request.Id);

            return mapper.Map<ProductBundleDto>(reloadedBundle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product bundle. BundleId: {BundleId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
