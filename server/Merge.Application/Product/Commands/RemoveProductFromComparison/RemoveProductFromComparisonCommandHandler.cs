using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.RemoveProductFromComparison;

public class RemoveProductFromComparisonCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RemoveProductFromComparisonCommandHandler> logger, ICacheService cache) : IRequestHandler<RemoveProductFromComparisonCommand, bool>
{

    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";

    public async Task<bool> Handle(RemoveProductFromComparisonCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing product from comparison. UserId: {UserId}, ProductId: {ProductId}",
            request.UserId, request.ProductId);

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

            // Check if product exists in comparison
            if (!comparison.Items.Any(i => i.ProductId == request.ProductId))
            {
                return false;
            }

            // Domain method içinde zaten product check ve removal var
            comparison.RemoveProduct(request.ProductId);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{comparison.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_MATRIX}{comparison.Id}", cancellationToken);

            logger.LogInformation("Product removed from comparison successfully. ComparisonId: {ComparisonId}, ProductId: {ProductId}",
                comparison.Id, request.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing product from comparison. UserId: {UserId}, ProductId: {ProductId}",
                request.UserId, request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
