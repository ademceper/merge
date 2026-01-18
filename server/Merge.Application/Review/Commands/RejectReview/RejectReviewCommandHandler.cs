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

namespace Merge.Application.Review.Commands.RejectReview;

public class RejectReviewCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RejectReviewCommandHandler> logger) : IRequestHandler<RejectReviewCommand, bool>
{

    public async Task<bool> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Rejecting review. ReviewId: {ReviewId}, Reason: {Reason}", 
            request.ReviewId, request.Reason);

        var review = await context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review is null)
        {
            return false;
        }

        var productId = review.ProductId;

        review.Reject(request.RejectedByUserId, request.Reason);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(productId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Review rejected successfully. ReviewId: {ReviewId}, Reason: {Reason}", 
            request.ReviewId, request.Reason);
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
