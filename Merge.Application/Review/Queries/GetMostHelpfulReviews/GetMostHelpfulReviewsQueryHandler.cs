using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Entities.Review;

namespace Merge.Application.Review.Queries.GetMostHelpfulReviews;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetMostHelpfulReviewsQueryHandler : IRequestHandler<GetMostHelpfulReviewsQuery, IEnumerable<ReviewHelpfulnessStatsDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMostHelpfulReviewsQueryHandler> _logger;
    private readonly ReviewSettings _reviewSettings;

    public GetMostHelpfulReviewsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetMostHelpfulReviewsQueryHandler> logger,
        IOptions<ReviewSettings> reviewSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _reviewSettings = reviewSettings.Value;
    }

    public async Task<IEnumerable<ReviewHelpfulnessStatsDto>> Handle(GetMostHelpfulReviewsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var limit = request.Limit > _reviewSettings.MaxHelpfulReviewsLimit
            ? _reviewSettings.MaxHelpfulReviewsLimit
            : request.Limit;
        if (limit < 1) limit = _reviewSettings.DefaultHelpfulReviewsLimit;

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Fetching most helpful reviews. ProductId: {ProductId}, Limit: {Limit}",
            request.ProductId, limit);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var reviews = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.ProductId == request.ProductId && r.IsApproved)
            .OrderByDescending(r => r.HelpfulCount)
            .ThenByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} most helpful reviews for product {ProductId}",
            reviews.Count, request.ProductId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var stats = _mapper.Map<IEnumerable<ReviewHelpfulnessStatsDto>>(reviews).ToList();
        foreach (var stat in stats)
        {
            stat.UserVote = null; // GetMostHelpfulReviews için user vote null
        }
        return stats;
    }
}
