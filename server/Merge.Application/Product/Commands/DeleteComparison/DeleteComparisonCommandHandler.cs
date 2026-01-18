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

namespace Merge.Application.Product.Commands.DeleteComparison;

public class DeleteComparisonCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteComparisonCommandHandler> logger, ICacheService cache) : IRequestHandler<DeleteComparisonCommand, bool>
{

    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";
    private const string CACHE_KEY_COMPARISON_BY_SHARE_CODE = "comparison_by_share_code_";

    public async Task<bool> Handle(DeleteComparisonCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting comparison. ComparisonId: {ComparisonId}, UserId: {UserId}",
            request.Id, request.UserId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comparison = await context.Set<ProductComparison>()
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == request.UserId, cancellationToken);

            if (comparison == null)
            {
                return false;
            }

            comparison.MarkAsDeleted();

            // Store share code for cache invalidation
            var shareCode = comparison.ShareCode;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_true_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_false_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_MATRIX}{request.Id}", cancellationToken);
            if (!string.IsNullOrEmpty(shareCode))
            {
                await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_SHARE_CODE}{shareCode}", cancellationToken);
            }

            logger.LogInformation("Comparison deleted successfully. ComparisonId: {ComparisonId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting comparison. ComparisonId: {ComparisonId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
