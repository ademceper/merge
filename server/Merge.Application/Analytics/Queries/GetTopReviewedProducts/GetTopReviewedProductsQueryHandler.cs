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
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetTopReviewedProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetTopReviewedProductsQueryHandler(
    IDbContext context,
    ILogger<GetTopReviewedProductsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetTopReviewedProductsQuery, List<TopReviewedProductDto>>
{

    public async Task<List<TopReviewedProductDto>> Handle(GetTopReviewedProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching top reviewed products. Limit: {Limit}", request.Limit);

        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var limit = request.Limit == 10 ? settings.Value.TopProductsLimit : request.Limit;
        
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        return await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.IsApproved)
            .GroupBy(r => new { r.ProductId, ProductName = r.Product.Name })
            .Select(g => new TopReviewedProductDto(
                g.Key.ProductId,
                g.Key.ProductName ?? string.Empty,
                g.Count(),
                Math.Round((decimal)g.Average(r => r.Rating), 2),
                g.Sum(r => r.HelpfulCount)
            ))
            .OrderByDescending(p => p.ReviewCount)
            .ThenByDescending(p => p.AverageRating)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

