using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.AssignSizeGuideToProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AssignSizeGuideToProductCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<AssignSizeGuideToProductCommandHandler> logger, ICacheService cache) : IRequestHandler<AssignSizeGuideToProductCommand>
{

    private const string CACHE_KEY_PRODUCT_SIZE_GUIDE = "product_size_guide_";
    private const string CACHE_KEY_SIZE_RECOMMENDATION = "size_recommendation_";

    public async Task Handle(AssignSizeGuideToProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning size guide to product. ProductId: {ProductId}, SizeGuideId: {SizeGuideId}",
            request.ProductId, request.SizeGuideId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await context.Set<ProductSizeGuide>()
                .FirstOrDefaultAsync(psg => psg.ProductId == request.ProductId, cancellationToken);

            if (existing != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                existing.Update(
                    request.SizeGuideId,
                    request.CustomNotes,
                    request.FitType,
                    request.FitDescription);
            }
            else
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
                var productSizeGuide = ProductSizeGuide.Create(
                    request.ProductId,
                    request.SizeGuideId,
                    request.CustomNotes,
                    request.FitType,
                    request.FitDescription);

                await context.Set<ProductSizeGuide>().AddAsync(productSizeGuide, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await cache.RemoveAsync($"{CACHE_KEY_PRODUCT_SIZE_GUIDE}{request.ProductId}", cancellationToken);
            // Note: Size recommendation cache includes measurements, so we can't invalidate all.
            // Cache expiration (30 min) will handle stale recommendations.

            logger.LogInformation("Size guide assigned to product successfully. ProductId: {ProductId}", request.ProductId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning size guide to product. ProductId: {ProductId}, SizeGuideId: {SizeGuideId}",
                request.ProductId, request.SizeGuideId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
