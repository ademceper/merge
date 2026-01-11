using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Review.Commands.ApproveReview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ApproveReviewCommandHandler : IRequestHandler<ApproveReviewCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveReviewCommandHandler> _logger;

    public ApproveReviewCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ApproveReviewCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving review. ReviewId: {ReviewId}", request.ReviewId);

        var review = await _context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        review.Approve(request.ApprovedByUserId);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review approved successfully. ReviewId: {ReviewId}, ProductId: {ProductId}", 
            request.ReviewId, review.ProductId);
        return true;
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query + Removed manual !r.IsDeleted (Global Query Filter)
        var reviewStats = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                AverageRating = g.Average(r => (decimal)r.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (reviewStats != null)
        {
            var product = await _context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product != null)
            {
                product.UpdateRating(reviewStats.AverageRating, reviewStats.Count);
            }
        }
    }
}
