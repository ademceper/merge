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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<ProductBundle>;

namespace Merge.Application.Product.Commands.PatchProductBundle;

/// <summary>
/// Handler for PatchProductBundleCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchProductBundleCommandHandler(
    IRepository bundleRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<PatchProductBundleCommandHandler> logger) : IRequestHandler<PatchProductBundleCommand, ProductBundleDto>
{
    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public async Task<ProductBundleDto> Handle(PatchProductBundleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching product bundle. BundleId: {BundleId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundle = await bundleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (bundle == null)
            {
                throw new NotFoundException("Paket", request.Id);
            }

            // Apply partial updates
            var name = request.PatchDto.Name ?? bundle.Name;
            var description = request.PatchDto.Description ?? bundle.Description;
            var bundlePrice = request.PatchDto.BundlePrice ?? bundle.BundlePrice;
            var imageUrl = request.PatchDto.ImageUrl ?? bundle.ImageUrl;
            var startDate = request.PatchDto.StartDate ?? bundle.StartDate;
            var endDate = request.PatchDto.EndDate ?? bundle.EndDate;

            bundle.Update(name, description, bundlePrice, bundle.OriginalTotalPrice, imageUrl, startDate, endDate);

            if (request.PatchDto.IsActive.HasValue)
            {
                if (request.PatchDto.IsActive.Value)
                {
                    bundle.Activate();
                }
                else
                {
                    bundle.Deactivate();
                }
            }

            await bundleRepository.UpdateAsync(bundle, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedBundle = await context.Set<ProductBundle>()
                .AsNoTracking()
                .Include(b => b.BundleItems)
                    .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (reloadedBundle == null)
            {
                logger.LogWarning("Product bundle {BundleId} not found after patch", request.Id);
                throw new NotFoundException("Paket", request.Id);
            }

            await cache.RemoveAsync($"{CACHE_KEY_BUNDLE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            logger.LogInformation("Product bundle patched successfully. BundleId: {BundleId}", request.Id);

            return mapper.Map<ProductBundleDto>(reloadedBundle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error patching product bundle. BundleId: {BundleId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
