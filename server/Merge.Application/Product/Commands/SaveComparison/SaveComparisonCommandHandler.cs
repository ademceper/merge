using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.SaveComparison;

public class SaveComparisonCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<SaveComparisonCommandHandler> logger, ICacheService cache) : IRequestHandler<SaveComparisonCommand, bool>
{

    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";

    public async Task<bool> Handle(SaveComparisonCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Saving comparison. UserId: {UserId}, Name: {Name}", request.UserId, request.Name);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comparison = await context.Set<ProductComparison>()
                .Where(c => c.UserId == request.UserId && !c.IsSaved)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (comparison == null)
            {
                return false;
            }

            comparison.Save(request.Name);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_true_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{comparison.Id}", cancellationToken);

            logger.LogInformation("Comparison saved successfully. ComparisonId: {ComparisonId}", comparison.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving comparison. UserId: {UserId}", request.UserId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
