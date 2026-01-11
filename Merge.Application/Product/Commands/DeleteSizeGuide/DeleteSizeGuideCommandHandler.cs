using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.DeleteSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteSizeGuideCommandHandler : IRequestHandler<DeleteSizeGuideCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSizeGuideCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_SIZE_GUIDE_BY_ID = "size_guide_";
    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";
    private const string CACHE_KEY_SIZE_GUIDES_BY_CATEGORY = "size_guides_by_category_";

    public DeleteSizeGuideCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteSizeGuideCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteSizeGuideCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting size guide. SizeGuideId: {SizeGuideId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sizeGuide = await _context.Set<SizeGuide>()
                .FirstOrDefaultAsync(sg => sg.Id == request.Id, cancellationToken);

            if (sizeGuide == null)
            {
                return false;
            }

            // Store category ID for cache invalidation
            var categoryId = sizeGuide.CategoryId;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            sizeGuide.MarkAsDeleted();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDE_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{categoryId}", cancellationToken);

            _logger.LogInformation("Size guide deleted successfully. SizeGuideId: {SizeGuideId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting size guide. SizeGuideId: {SizeGuideId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
