using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.RemoveSizeGuideFromProduct;

public class RemoveSizeGuideFromProductCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RemoveSizeGuideFromProductCommandHandler> logger, ICacheService cache) : IRequestHandler<RemoveSizeGuideFromProductCommand, bool>
{

    private const string CACHE_KEY_PRODUCT_SIZE_GUIDE = "product_size_guide_";
    private const string CACHE_KEY_SIZE_RECOMMENDATION = "size_recommendation_";

    public async Task<bool> Handle(RemoveSizeGuideFromProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing size guide from product. ProductId: {ProductId}", request.ProductId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var productSizeGuide = await context.Set<ProductSizeGuide>()
                .FirstOrDefaultAsync(psg => psg.ProductId == request.ProductId, cancellationToken);

            if (productSizeGuide == null)
            {
                return false;
            }

            productSizeGuide.MarkAsDeleted();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_PRODUCT_SIZE_GUIDE}{request.ProductId}", cancellationToken);
            // Note: Size recommendation cache includes measurements, so we can't invalidate all.
            // Cache expiration (30 min) will handle stale recommendations.

            logger.LogInformation("Size guide removed from product successfully. ProductId: {ProductId}", request.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing size guide from product. ProductId: {ProductId}", request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
