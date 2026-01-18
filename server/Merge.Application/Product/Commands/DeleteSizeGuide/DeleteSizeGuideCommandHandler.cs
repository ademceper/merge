using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.DeleteSizeGuide;

public class DeleteSizeGuideCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteSizeGuideCommandHandler> logger, ICacheService cache) : IRequestHandler<DeleteSizeGuideCommand, bool>
{

    private const string CACHE_KEY_SIZE_GUIDE_BY_ID = "size_guide_";
    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";
    private const string CACHE_KEY_SIZE_GUIDES_BY_CATEGORY = "size_guides_by_category_";

    public async Task<bool> Handle(DeleteSizeGuideCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting size guide. SizeGuideId: {SizeGuideId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sizeGuide = await context.Set<SizeGuide>()
                .FirstOrDefaultAsync(sg => sg.Id == request.Id, cancellationToken);

            if (sizeGuide == null)
            {
                return false;
            }

            // Store category ID for cache invalidation
            var categoryId = sizeGuide.CategoryId;

            sizeGuide.MarkAsDeleted();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{categoryId}", cancellationToken);

            logger.LogInformation("Size guide deleted successfully. SizeGuideId: {SizeGuideId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting size guide. SizeGuideId: {SizeGuideId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
