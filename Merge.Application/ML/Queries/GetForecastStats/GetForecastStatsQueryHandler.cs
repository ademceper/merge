using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.ML.Queries.GetForecastStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetForecastStatsQueryHandler : IRequestHandler<GetForecastStatsQuery, DemandForecastStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetForecastStatsQueryHandler> _logger;
    private readonly MLSettings _mlSettings;

    public GetForecastStatsQueryHandler(
        IDbContext context,
        ILogger<GetForecastStatsQueryHandler> logger,
        IOptions<MLSettings> mlSettings)
    {
        _context = context;
        _logger = logger;
        _mlSettings = mlSettings.Value;
    }

    public async Task<DemandForecastStatsDto> Handle(GetForecastStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting forecast stats. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-_mlSettings.DefaultAnalysisPeriodDays);
        var end = request.EndDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !oi.Order.IsDeleted (Global Query Filter)
        var productsWithSales = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt <= end)
            .Select(oi => oi.ProductId)
            .Distinct()
            .CountAsync(cancellationToken);

        _logger.LogInformation("Forecast stats retrieved successfully.");

        return new DemandForecastStatsDto(
            totalProducts,
            productsWithSales,
            totalProducts > 0 ? (decimal)productsWithSales / totalProducts * 100 : 0,
            start,
            end
        );
    }
}
