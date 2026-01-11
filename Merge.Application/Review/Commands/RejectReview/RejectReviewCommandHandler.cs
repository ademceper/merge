using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Review.Commands.RejectReview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RejectReviewCommandHandler : IRequestHandler<RejectReviewCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectReviewCommandHandler> _logger;

    public RejectReviewCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RejectReviewCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting review. ReviewId: {ReviewId}, Reason: {Reason}", 
            request.ReviewId, request.Reason);

        var review = await _context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            return false;
        }

        var productId = review.ProductId;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        review.Reject(request.RejectedByUserId, request.Reason);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(productId, cancellationToken);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review rejected successfully. ReviewId: {ReviewId}, Reason: {Reason}", 
            request.ReviewId, request.Reason);
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
