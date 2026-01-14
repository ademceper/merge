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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReviewCommandHandler> _logger;

    public DeleteReviewCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteReviewCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting review. ReviewId: {ReviewId}, UserId: {UserId}", request.ReviewId, request.UserId);

        var review = await _context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            return false;
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi review'lerini silebilmeli
        // Not: Admin kontrolü controller'da yapılıyor, burada sadece UserId kontrolü yapılıyor
        if (review.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu değerlendirmeyi silme yetkiniz yok.");
        }

        var productId = review.ProductId;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        review.MarkAsDeleted();

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(productId, cancellationToken);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review deleted successfully. ReviewId: {ReviewId}, ProductId: {ProductId}", 
            request.ReviewId, productId);
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
