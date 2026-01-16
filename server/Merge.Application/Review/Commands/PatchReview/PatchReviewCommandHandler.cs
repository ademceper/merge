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
public class PatchReviewCommandHandler : IRequestHandler<PatchReviewCommand, ReviewDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PatchReviewCommandHandler> _logger;

    public PatchReviewCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PatchReviewCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReviewDto> Handle(PatchReviewCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Patching review. ReviewId: {ReviewId}, UserId: {UserId}", request.ReviewId, request.UserId);

        var review = await _context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi review'lerini güncelleyebilmeli
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
            _logger.LogInformation("Review rating updated. ReviewId: {ReviewId}, OldRating: {OldRating}, NewRating: {NewRating}",
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Single query instead of multiple LoadAsync calls
        review = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        _logger.LogInformation("Review patched successfully. ReviewId: {ReviewId}", request.ReviewId);

        return _mapper.Map<ReviewDto>(review!);
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var reviews = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
        {
            return;
        }

        var averageRating = reviews.Average(r => r.Rating);
        var product = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product != null)
        {
            var rating = new Rating((int)Math.Round(averageRating));
            product.UpdateAverageRating(rating);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
