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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RemoveSizeGuideFromProductCommandHandler : IRequestHandler<RemoveSizeGuideFromProductCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveSizeGuideFromProductCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PRODUCT_SIZE_GUIDE = "product_size_guide_";
    private const string CACHE_KEY_SIZE_RECOMMENDATION = "size_recommendation_";

    public RemoveSizeGuideFromProductCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RemoveSizeGuideFromProductCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(RemoveSizeGuideFromProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing size guide from product. ProductId: {ProductId}", request.ProductId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var productSizeGuide = await _context.Set<ProductSizeGuide>()
                .FirstOrDefaultAsync(psg => psg.ProductId == request.ProductId, cancellationToken);

            if (productSizeGuide == null)
            {
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            productSizeGuide.MarkAsDeleted();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_PRODUCT_SIZE_GUIDE}{request.ProductId}", cancellationToken);
            // Note: Size recommendation cache includes measurements, so we can't invalidate all.
            // Cache expiration (30 min) will handle stale recommendations.

            _logger.LogInformation("Size guide removed from product successfully. ProductId: {ProductId}", request.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing size guide from product. ProductId: {ProductId}", request.ProductId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
