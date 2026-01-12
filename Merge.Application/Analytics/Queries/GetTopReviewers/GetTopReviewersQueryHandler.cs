using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetTopReviewers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetTopReviewersQueryHandler : IRequestHandler<GetTopReviewersQuery, List<ReviewerStatsDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetTopReviewersQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;

    public GetTopReviewersQueryHandler(
        IDbContext context,
        ILogger<GetTopReviewersQueryHandler> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<List<ReviewerStatsDto>> Handle(GetTopReviewersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching top reviewers. Limit: {Limit}", request.Limit);

        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var limit = request.Limit == 10 ? _settings.TopProductsLimit : request.Limit;
        
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        return await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.IsApproved)
            .GroupBy(r => new { r.UserId, r.User.FirstName, r.User.LastName })
            .Select(g => new ReviewerStatsDto(
                g.Key.UserId,
                $"{g.Key.FirstName} {g.Key.LastName}",
                g.Count(),
                Math.Round((decimal)g.Average(r => r.Rating), 2),
                g.Sum(r => r.HelpfulCount)
            ))
            .OrderByDescending(r => r.ReviewCount)
            .ThenByDescending(r => r.HelpfulVotes)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

