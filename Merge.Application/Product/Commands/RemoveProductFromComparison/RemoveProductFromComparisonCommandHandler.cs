using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.RemoveProductFromComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RemoveProductFromComparisonCommandHandler : IRequestHandler<RemoveProductFromComparisonCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveProductFromComparisonCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";

    public RemoveProductFromComparisonCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RemoveProductFromComparisonCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(RemoveProductFromComparisonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing product from comparison. UserId: {UserId}, ProductId: {ProductId}",
            request.UserId, request.ProductId);

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

            // Check if product exists in comparison
            if (!comparison.Items.Any(i => i.ProductId == request.ProductId))
            {
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // Domain method içinde zaten product check ve removal var
            comparison.RemoveProduct(request.ProductId);

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{comparison.Id}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_MATRIX}{comparison.Id}", cancellationToken);

            _logger.LogInformation("Product removed from comparison successfully. ComparisonId: {ComparisonId}, ProductId: {ProductId}",
                comparison.Id, request.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product from comparison. UserId: {UserId}, ProductId: {ProductId}",
                request.UserId, request.ProductId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
