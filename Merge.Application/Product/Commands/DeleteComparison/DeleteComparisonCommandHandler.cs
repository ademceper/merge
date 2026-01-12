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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteComparisonCommandHandler : IRequestHandler<DeleteComparisonCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteComparisonCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";
    private const string CACHE_KEY_COMPARISON_BY_SHARE_CODE = "comparison_by_share_code_";

    public DeleteComparisonCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteComparisonCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteComparisonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting comparison. ComparisonId: {ComparisonId}, UserId: {UserId}",
            request.Id, request.UserId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comparison = await _context.Set<ProductComparison>()
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == request.UserId, cancellationToken);

            if (comparison == null)
            {
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            comparison.MarkAsDeleted();

            // Store share code for cache invalidation
            var shareCode = comparison.ShareCode;

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_true_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_false_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_MATRIX}{request.Id}", cancellationToken);
            if (!string.IsNullOrEmpty(shareCode))
            {
                await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_SHARE_CODE}{shareCode}", cancellationToken);
            }

            _logger.LogInformation("Comparison deleted successfully. ComparisonId: {ComparisonId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comparison. ComparisonId: {ComparisonId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
