using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.ApproveReview;

public class ApproveReviewCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ApproveReviewCommandHandler> logger) : IRequestHandler<ApproveReviewCommand, bool>
{

    public async Task<bool> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving review. ReviewId: {ReviewId}", request.ReviewId);

        var review = await context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review is null)
        {
            return false;
        }

        review.Approve(request.ApprovedByUserId);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Review approved successfully. ReviewId: {ReviewId}, ProductId: {ProductId}", 
            request.ReviewId, review.ProductId);
        return true;
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviewStats = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                AverageRating = g.Average(r => (decimal)r.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (reviewStats is not null)
        {
            var product = await context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product is not null)
            {
                product.UpdateRating(reviewStats.AverageRating, reviewStats.Count);
            }
        }
    }
}
