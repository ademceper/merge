using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.ValueObjects;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.PatchReview;

/// <summary>
/// Handler for PatchReviewCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchReviewCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchReviewCommandHandler> logger) : IRequestHandler<PatchReviewCommand, ReviewDto>
{

    public async Task<ReviewDto> Handle(PatchReviewCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching review. ReviewId: {ReviewId}, UserId: {UserId}", request.ReviewId, request.UserId);

        var review = await context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        if (review.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu değerlendirmeyi güncelleme yetkiniz yok.");
        }

        // Apply partial updates - only update fields that are provided
        if (request.PatchDto.Rating.HasValue)
        {
            var oldRating = review.Rating;
            var newRating = new Rating(request.PatchDto.Rating.Value);
            review.UpdateRating(newRating);
            logger.LogInformation("Review rating updated. ReviewId: {ReviewId}, OldRating: {OldRating}, NewRating: {NewRating}",
                request.ReviewId, oldRating, request.PatchDto.Rating.Value);
        }

        if (request.PatchDto.Title != null)
        {
            review.UpdateTitle(request.PatchDto.Title);
        }

        if (request.PatchDto.Comment != null)
        {
            review.UpdateComment(request.PatchDto.Comment);
        }

        // Güncelleme sonrası tekrar onay gerekli - Reject() ile pending yap
        if (review.IsApproved && (request.PatchDto.Rating.HasValue || request.PatchDto.Title != null || request.PatchDto.Comment != null))
        {
            review.Reject(Guid.Empty, "Review updated, requires re-approval");
        }

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        logger.LogInformation("Review patched successfully. ReviewId: {ReviewId}", request.ReviewId);

        return mapper.Map<ReviewDto>(review!);
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
        {
            return;
        }

        var averageRating = (decimal)reviews.Average(r => r.Rating);
        var product = await context.Set<ProductEntity>()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product != null)
        {
            product.UpdateRating(averageRating, reviews.Count);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
