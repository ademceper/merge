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
public class AssignSizeGuideToProductCommandHandler : IRequestHandler<AssignSizeGuideToProductCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignSizeGuideToProductCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PRODUCT_SIZE_GUIDE = "product_size_guide_";
    private const string CACHE_KEY_SIZE_RECOMMENDATION = "size_recommendation_";

    public AssignSizeGuideToProductCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<AssignSizeGuideToProductCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task Handle(AssignSizeGuideToProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning size guide to product. ProductId: {ProductId}, SizeGuideId: {SizeGuideId}",
            request.ProductId, request.SizeGuideId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await _context.Set<ProductSizeGuide>()
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

                await _context.Set<ProductSizeGuide>().AddAsync(productSizeGuide, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_PRODUCT_SIZE_GUIDE}{request.ProductId}", cancellationToken);
            // Note: Size recommendation cache includes measurements, so we can't invalidate all.
            // Cache expiration (30 min) will handle stale recommendations.

            _logger.LogInformation("Size guide assigned to product successfully. ProductId: {ProductId}", request.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning size guide to product. ProductId: {ProductId}, SizeGuideId: {SizeGuideId}",
                request.ProductId, request.SizeGuideId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
