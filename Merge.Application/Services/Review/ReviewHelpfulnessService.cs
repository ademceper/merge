using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Infrastructure.Repositories;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Review;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Services.Review;

public class ReviewHelpfulnessService : IReviewHelpfulnessService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewHelpfulnessService> _logger;

    public ReviewHelpfulnessService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReviewHelpfulnessService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task MarkReviewHelpfulnessAsync(Guid userId, MarkReviewHelpfulnessDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Review helpfulness işaretleniyor. UserId: {UserId}, ReviewId: {ReviewId}, IsHelpful: {IsHelpful}",
            userId, dto.ReviewId, dto.IsHelpful);

        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var review = await _context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == dto.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException("Değerlendirme", dto.ReviewId);
        }

        // ✅ PERFORMANCE: Removed manual !rh.IsDeleted (Global Query Filter)
        var existingVote = await _context.Set<ReviewHelpfulness>()
            .FirstOrDefaultAsync(rh => rh.ReviewId == dto.ReviewId && rh.UserId == userId, cancellationToken);

        if (existingVote != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Update existing vote
            if (existingVote.IsHelpful != dto.IsHelpful)
            {
                // Decrement old count
                if (existingVote.IsHelpful)
                    review.UnmarkAsHelpful();
                else
                    review.UnmarkAsUnhelpful();

                // Increment new count
                if (dto.IsHelpful)
                    review.MarkAsHelpful();
                else
                    review.MarkAsUnhelpful();

                existingVote.IsHelpful = dto.IsHelpful;
            }
        }
        else
        {
            // Create new vote
            var vote = new ReviewHelpfulness
            {
                ReviewId = dto.ReviewId,
                UserId = userId,
                IsHelpful = dto.IsHelpful
            };

            await _context.Set<ReviewHelpfulness>().AddAsync(vote, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Increment count
            if (dto.IsHelpful)
                review.MarkAsHelpful();
            else
                review.MarkAsUnhelpful();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Review helpfulness işaretlendi. UserId: {UserId}, ReviewId: {ReviewId}, IsHelpful: {IsHelpful}",
            userId, dto.ReviewId, dto.IsHelpful);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task RemoveHelpfulnessVoteAsync(Guid userId, Guid reviewId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !rh.IsDeleted (Global Query Filter)
        var vote = await _context.Set<ReviewHelpfulness>()
            .FirstOrDefaultAsync(rh => rh.ReviewId == reviewId && rh.UserId == userId, cancellationToken);

        if (vote == null) return;

        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var review = await _context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Decrement count
            if (vote.IsHelpful)
                review.UnmarkAsHelpful();
            else
                review.UnmarkAsUnhelpful();
        }

        vote.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Review helpfulness oyu kaldırıldı. UserId: {UserId}, ReviewId: {ReviewId}",
            userId, reviewId);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReviewHelpfulnessStatsDto> GetReviewHelpfulnessStatsAsync(Guid reviewId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var review = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException("Değerlendirme", reviewId);
        }

        bool? userVote = null;
        if (userId.HasValue)
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !rh.IsDeleted (Global Query Filter)
            var vote = await _context.Set<ReviewHelpfulness>()
                .AsNoTracking()
                .FirstOrDefaultAsync(rh => rh.ReviewId == reviewId && rh.UserId == userId.Value, cancellationToken);

            if (vote != null)
            {
                userVote = vote.IsHelpful;
            }
        }

        var totalVotes = review.HelpfulCount + review.UnhelpfulCount;
        var helpfulPercentage = totalVotes > 0 ? (decimal)review.HelpfulCount / totalVotes * 100 : 0;

        return new ReviewHelpfulnessStatsDto
        {
            ReviewId = reviewId,
            HelpfulCount = review.HelpfulCount,
            UnhelpfulCount = review.UnhelpfulCount,
            TotalVotes = totalVotes,
            HelpfulPercentage = Math.Round(helpfulPercentage, 2),
            UserVote = userVote
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ReviewHelpfulnessStatsDto>> GetMostHelpfulReviewsAsync(Guid productId, int limit = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var reviews = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.HelpfulCount)
            .ThenByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: UserVote null olarak set ediliyor (GetMostHelpfulReviewsAsync için)
        var stats = _mapper.Map<IEnumerable<ReviewHelpfulnessStatsDto>>(reviews).ToList();
        // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK - ama bu sadece property assignment (minimal)
        foreach (var stat in stats)
        {
            stat.UserVote = null;
        }
        return stats;
    }
}
