using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetReviewTrends;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetReviewTrendsQueryHandler : IRequestHandler<GetReviewTrendsQuery, List<ReviewTrendDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetReviewTrendsQueryHandler> _logger;

    public GetReviewTrendsQueryHandler(
        IDbContext context,
        ILogger<GetReviewTrendsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ReviewTrendDto>> Handle(GetReviewTrendsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching review trends. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        return await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.IsApproved && r.CreatedAt >= request.StartDate && r.CreatedAt <= request.EndDate)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new ReviewTrendDto(
                g.Key,
                g.Count(),
                Math.Round((decimal)g.Average(r => r.Rating), 2)
            ))
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);
    }
}

