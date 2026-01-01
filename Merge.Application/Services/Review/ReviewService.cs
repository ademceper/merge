using AutoMapper;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Review;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Review;
using Microsoft.Extensions.Logging;


namespace Merge.Application.Services.Review;

public class ReviewService : IReviewService
{
    private readonly IRepository<ReviewEntity> _reviewRepository;
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IRepository<ReviewEntity> reviewRepository,
        IRepository<ProductEntity> productRepository,
        ApplicationDbContext context,
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

    public async Task<ReviewDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var review = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return _mapper.Map<ReviewDto>(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted check
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation(
            "Retrieved {Count} reviews for product {ProductId}, page {Page}",
            reviews.Count, productId, page);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
    }

    public async Task<IEnumerable<ReviewDto>> GetByUserIdAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted check
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        _logger.LogInformation(
            "Retrieved {Count} reviews for user {UserId}",
            reviews.Count, userId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
    }

    public async Task<ReviewDto> CreateAsync(CreateReviewDto dto)
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
        var hasOrder = await _context.OrderItems
            .AnyAsync(oi => oi.ProductId == dto.ProductId &&
                          oi.Order.UserId == dto.UserId &&
                          oi.Order.PaymentStatus == PaymentStatus.Completed);

        var review = new ReviewEntity
        {
            UserId = dto.UserId,
            ProductId = dto.ProductId,
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment,
            IsVerifiedPurchase = hasOrder,
            IsApproved = false // Admin onayı gerekli
        };

        review = await _reviewRepository.AddAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(dto.ProductId);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Single query instead of multiple LoadAsync calls
        review = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == review.Id);

        _logger.LogInformation(
            "Review created. ReviewId: {ReviewId}, ProductId: {ProductId}, UserId: {UserId}, Rating: {Rating}",
            review!.Id, dto.ProductId, dto.UserId, dto.Rating);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return _mapper.Map<ReviewDto>(review);
    }

    public async Task<ReviewDto> UpdateAsync(Guid id, UpdateReviewDto dto)
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

        var oldRating = review.Rating;
        review.Rating = dto.Rating;
        review.Title = dto.Title;
        review.Comment = dto.Comment;
        review.IsApproved = false; // Güncelleme sonrası tekrar onay gerekli

        await _reviewRepository.UpdateAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Single query instead of multiple LoadAsync calls
        review = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        _logger.LogInformation(
            "Review updated. ReviewId: {ReviewId}, OldRating: {OldRating}, NewRating: {NewRating}",
            id, oldRating, dto.Rating);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserName ve ProductName AutoMapper'da map edilmeli
        return _mapper.Map<ReviewDto>(review!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            return false;
        }

        var productId = review.ProductId;
        await _reviewRepository.DeleteAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(productId);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review deleted. ReviewId: {ReviewId}, ProductId: {ProductId}", id, productId);
        return true;
    }

    public async Task<bool> ApproveReviewAsync(Guid id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            return false;
        }

        review.IsApproved = true;
        await _reviewRepository.UpdateAsync(review);

        // Ürün rating'ini güncelle
        await UpdateProductRatingAsync(review.ProductId);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review approved. ReviewId: {ReviewId}, ProductId: {ProductId}", id, review.ProductId);
        return true;
    }

    public async Task<bool> RejectReviewAsync(Guid id, string reason)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            return false;
        }

        var productId = review.ProductId;
        await _reviewRepository.DeleteAsync(review);

        // ✅ CRITICAL: Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review rejected. ReviewId: {ReviewId}, Reason: {Reason}", id, reason);
        return true;
    }

    private async Task UpdateProductRatingAsync(Guid productId)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter)
        // ✅ PERFORMANCE: Use server-side aggregation instead of loading all reviews
        var reviewStats = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                AverageRating = g.Average(r => (decimal)r.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        if (reviewStats != null)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null)
            {
                product.Rating = reviewStats.AverageRating;
                product.ReviewCount = reviewStats.Count;
                await _productRepository.UpdateAsync(product);
            }
        }
    }
}

