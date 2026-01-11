using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.ClearComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ClearComparisonCommandHandler : IRequestHandler<ClearComparisonCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearComparisonCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";

    public ClearComparisonCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ClearComparisonCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(ClearComparisonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clearing comparison. UserId: {UserId}", request.UserId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comparison = await _context.Set<ProductComparison>()
                .Include(c => c.Items)
                .Where(c => c.UserId == request.UserId && !c.IsSaved)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (comparison == null)
            {
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // Clear all products from comparison using domain method
            var productIds = comparison.Items.Select(i => i.ProductId).ToList();
            foreach (var productId in productIds)
            {
                comparison.RemoveProduct(productId);
            }

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{comparison.Id}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_MATRIX}{comparison.Id}", cancellationToken);

            _logger.LogInformation("Comparison cleared successfully. ComparisonId: {ComparisonId}", comparison.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing comparison. UserId: {UserId}", request.UserId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
