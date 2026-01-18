using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.ClearComparison;

public class ClearComparisonCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ClearComparisonCommandHandler> logger, ICacheService cache) : IRequestHandler<ClearComparisonCommand, bool>
{

    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";

    public async Task<bool> Handle(ClearComparisonCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Clearing comparison. UserId: {UserId}", request.UserId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comparison = await context.Set<ProductComparison>()
                .Include(c => c.Items)
                .Where(c => c.UserId == request.UserId && !c.IsSaved)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (comparison == null)
            {
                return false;
            }

            // Clear all products from comparison using domain method
            var productIds = comparison.Items.Select(i => i.ProductId).ToList();
            foreach (var productId in productIds)
            {
                comparison.RemoveProduct(productId);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{comparison.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_MATRIX}{comparison.Id}", cancellationToken);

            logger.LogInformation("Comparison cleared successfully. ComparisonId: {ComparisonId}", comparison.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing comparison. UserId: {UserId}", request.UserId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
