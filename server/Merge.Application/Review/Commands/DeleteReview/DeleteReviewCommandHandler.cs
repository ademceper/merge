using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.DeleteReview;

public class DeleteReviewCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteReviewCommandHandler> logger) : IRequestHandler<DeleteReviewCommand, bool>
{

    public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting review. ReviewId: {ReviewId}, UserId: {UserId}", request.ReviewId, request.UserId);

        var review = await context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            return false;
        }

        // Not: Admin kontrolü controller'da yapılıyor, burada sadece UserId kontrolü yapılıyor
        if (review.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu değerlendirmeyi silme yetkiniz yok.");
        }

        var productId = review.ProductId;

        review.MarkAsDeleted();

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(productId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Review deleted successfully. ReviewId: {ReviewId}, ProductId: {ProductId}", 
            request.ReviewId, productId);
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

        if (reviewStats != null)
        {
            var product = await context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product != null)
            {
                product.UpdateRating(reviewStats.AverageRating, reviewStats.Count);
            }
        }
    }
}
