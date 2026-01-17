using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.UpdateReview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateReviewCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateReviewCommandHandler> logger) : IRequestHandler<UpdateReviewCommand, ReviewDto>
{

    public async Task<ReviewDto> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating review. ReviewId: {ReviewId}, UserId: {UserId}", request.ReviewId, request.UserId);

        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, handler'da tekrar validation gereksiz

        var review = await context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi review'lerini güncelleyebilmeli
        // Not: Admin kontrolü controller'da yapılıyor, burada sadece UserId kontrolü yapılıyor
        if (review.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu değerlendirmeyi güncelleme yetkiniz yok.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        var oldRating = review.Rating;
        var newRating = new Rating(request.Rating);
        review.UpdateRating(newRating);
        review.UpdateTitle(request.Title);
        review.UpdateComment(request.Comment);

        // Güncelleme sonrası tekrar onay gerekli - Reject() ile pending yap
        // Not: UpdateReviewCommand'da ApprovedByUserId yok, bu yüzden Guid.Empty kullanıyoruz
        // Bu durumda review otomatik olarak pending'e döner
        if (review.IsApproved)
        {
            review.Reject(Guid.Empty, "Review updated, requires re-approval");
        }

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        logger.LogInformation(
            "Review updated successfully. ReviewId: {ReviewId}, OldRating: {OldRating}, NewRating: {NewRating}",
            request.ReviewId, oldRating, request.Rating);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<ReviewDto>(review!);
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query + Removed manual !r.IsDeleted (Global Query Filter)
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
