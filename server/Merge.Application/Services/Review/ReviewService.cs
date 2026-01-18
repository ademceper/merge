using AutoMapper;
using UserEntity = Merge.Domain.Modules.Identity.User;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Review;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Application.DTOs.Review;
using Merge.Application.Common;
using Microsoft.Extensions.Logging;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IReviewRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Review>;
using IProductRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Product>;

namespace Merge.Application.Services.Review;

public class ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository, IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReviewService> logger) : IReviewService
{

    public async Task<ReviewDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (review is null) return null;

        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return mapper.Map<ReviewDto>(review);
    }

    public async Task<PagedResult<ReviewDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId && r.IsApproved);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Retrieved {Count} reviews for product {ProductId}, page {Page}, pageSize {PageSize}, totalCount {TotalCount}",
            reviews.Count, productId, page, pageSize, totalCount);

        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        var reviewDtos = mapper.Map<IEnumerable<ReviewDto>>(reviews);

        return new PagedResult<ReviewDto>
        {
            Items = reviewDtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReviewDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100; // Max limit

        var query = context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Retrieved {Count} reviews for user {UserId}, page {Page}, pageSize {PageSize}",
            reviews.Count, userId, page, pageSize);

        var reviewDtos = mapper.Map<IEnumerable<ReviewDto>>(reviews);

        return new PagedResult<ReviewDto>
        {
            Items = reviewDtos.ToList(), // ✅ IEnumerable -> List'e çevir
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReviewDto> CreateAsync(CreateReviewDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ValidationException("Puan 1 ile 5 arasında olmalıdır.");
        }

        if (string.IsNullOrWhiteSpace(dto.Comment))
        {
            throw new ValidationException("Yorum boş olamaz.");
        }

        var hasOrder = await context.Set<OrderItem>()
            .AnyAsync(oi => oi.ProductId == dto.ProductId &&
                          oi.Order.UserId == dto.UserId &&
                          oi.Order.PaymentStatus == PaymentStatus.Completed, cancellationToken);

        var rating = new Rating(dto.Rating);
        var review = ReviewEntity.Create(
            dto.UserId,
            dto.ProductId,
            rating,
            dto.Title,
            dto.Comment,
            hasOrder
        );

        review = await reviewRepository.AddAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(dto.ProductId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == review.Id, cancellationToken);

        if (review is null)
        {
            logger.LogError("Review not found after creation. ReviewId: {ReviewId}", review?.Id);
            throw new InvalidOperationException("Review could not be retrieved after creation");
        }

        logger.LogInformation(
            "Review created. ReviewId: {ReviewId}, ProductId: {ProductId}, UserId: {UserId}, Rating: {Rating}",
            review.Id, dto.ProductId, dto.UserId, dto.Rating);

        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return mapper.Map<ReviewDto>(review);
    }

    public async Task<ReviewDto> UpdateAsync(Guid id, UpdateReviewDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ValidationException("Puan 1 ile 5 arasında olmalıdır.");
        }

        var review = await reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            throw new NotFoundException("Değerlendirme", id);
        }

        var oldRating = review.Rating; // ✅ Rating is int in Review entity
        var newRating = new Rating(dto.Rating);
        review.UpdateRating(newRating);
        review.UpdateTitle(dto.Title);
        review.UpdateComment(dto.Comment);
        // Güncelleme sonrası tekrar onay gerekli - Reject() ile pending yap
        if (review.IsApproved)
        {
            // TODO: RejectedByUserId parametresi eklenmeli
            review.Reject(Guid.Empty, "Updated by user");
        }

        await reviewRepository.UpdateAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        logger.LogInformation(
            "Review updated. ReviewId: {ReviewId}, OldRating: {OldRating}, NewRating: {NewRating}",
            id, oldRating, dto.Rating);

        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return mapper.Map<ReviewDto>(review!);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            return false;
        }

        var productId = review.ProductId;
        await reviewRepository.DeleteAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(productId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Review deleted. ReviewId: {ReviewId}, ProductId: {ProductId}", id, productId);
        return true;
    }

    public async Task<bool> ApproveReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            return false;
        }

        // TODO: ApprovedByUserId parametresi eklenmeli
        review.Approve(Guid.Empty);
        await reviewRepository.UpdateAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Review approved. ReviewId: {ReviewId}, ProductId: {ProductId}", id, review.ProductId);
        return true;
    }

    public async Task<bool> RejectReviewAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var review = await reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            return false;
        }

        var productId = review.ProductId;
        await reviewRepository.DeleteAsync(review);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Review rejected. ReviewId: {ReviewId}, Reason: {Reason}", id, reason);
        return true;
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviewStats = await context.Set<ReviewEntity>()
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
            var product = await productRepository.GetByIdAsync(productId);
            if (product is not null)
            {
                product.UpdateRating(reviewStats.AverageRating, reviewStats.Count);
                await productRepository.UpdateAsync(product);
            }
        }
    }
}

