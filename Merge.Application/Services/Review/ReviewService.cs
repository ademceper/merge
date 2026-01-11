using AutoMapper;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
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


namespace Merge.Application.Services.Review;

public class ReviewService : IReviewService
{
    private readonly IRepository<ReviewEntity> _reviewRepository;
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IRepository<ReviewEntity> reviewRepository,
        IRepository<ProductEntity> productRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReviewDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var review = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (review == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return _mapper.Map<ReviewDto>(review);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ReviewDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted check (Global Query Filter)
        var query = _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId && r.IsApproved);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Retrieved {Count} reviews for product {ProductId}, page {Page}, pageSize {PageSize}, totalCount {TotalCount}",
            reviews.Count, productId, page, pageSize, totalCount);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

        return new PagedResult<ReviewDto>
        {
            Items = reviewDtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ PERFORMANCE: Pagination ekle (BEST_PRACTICES_ANALIZI.md - BOLUM 3.1.4)
    public async Task<PagedResult<ReviewDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted check
        var query = _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} reviews for user {UserId}, page {Page}, pageSize {PageSize}",
            reviews.Count, userId, page, pageSize);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

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
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ValidationException("Puan 1 ile 5 arasında olmalıdır.");
        }

        if (string.IsNullOrWhiteSpace(dto.Comment))
        {
            throw new ValidationException("Yorum boş olamaz.");
        }

        // ✅ PERFORMANCE: Removed manual !oi.Order.IsDeleted check (Global Query Filter)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var hasOrder = await _context.Set<OrderItem>()
            .AnyAsync(oi => oi.ProductId == dto.ProductId &&
                          oi.Order.UserId == dto.UserId &&
                          oi.Order.PaymentStatus == PaymentStatus.Completed, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
        var rating = new Rating(dto.Rating);
        var review = ReviewEntity.Create(
            dto.UserId,
            dto.ProductId,
            rating,
            dto.Title,
            dto.Comment,
            hasOrder
        );

        review = await _reviewRepository.AddAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(dto.ProductId, cancellationToken);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Single query instead of multiple LoadAsync calls
        review = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == review.Id, cancellationToken);

        _logger.LogInformation(
            "Review created. ReviewId: {ReviewId}, ProductId: {ProductId}, UserId: {UserId}, Rating: {Rating}",
            review!.Id, dto.ProductId, dto.UserId, dto.Rating);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return _mapper.Map<ReviewDto>(review);
    }

    public async Task<ReviewDto> UpdateAsync(Guid id, UpdateReviewDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ValidationException("Puan 1 ile 5 arasında olmalıdır.");
        }

        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            throw new NotFoundException("Değerlendirme", id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
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

        await _reviewRepository.UpdateAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Single query instead of multiple LoadAsync calls
        review = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        _logger.LogInformation(
            "Review updated. ReviewId: {ReviewId}, OldRating: {OldRating}, NewRating: {NewRating}",
            id, oldRating, dto.Rating);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return _mapper.Map<ReviewDto>(review!);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            return false;
        }

        var productId = review.ProductId;
        await _reviewRepository.DeleteAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(productId, cancellationToken);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review deleted. ReviewId: {ReviewId}, ProductId: {ProductId}", id, productId);
        return true;
    }

    public async Task<bool> ApproveReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        // TODO: ApprovedByUserId parametresi eklenmeli
        review.Approve(Guid.Empty);
        await _reviewRepository.UpdateAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review approved. ReviewId: {ReviewId}, ProductId: {ProductId}", id, review.ProductId);
        return true;
    }

    public async Task<bool> RejectReviewAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            return false;
        }

        var productId = review.ProductId;
        await _reviewRepository.DeleteAsync(review);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review rejected. ReviewId: {ReviewId}, Reason: {Reason}", id, reason);
        return true;
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter)
        // ✅ PERFORMANCE: Use server-side aggregation instead of loading all reviews
        var reviewStats = await _context.Set<ReviewEntity>()
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
            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                product.UpdateRating(reviewStats.AverageRating, reviewStats.Count);
                await _productRepository.UpdateAsync(product);
            }
        }
    }
}

