using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.GetForecastStats;

public class GetForecastStatsQueryHandler(IDbContext context, ILogger<GetForecastStatsQueryHandler> logger, IOptions<MLSettings> mlSettings) : IRequestHandler<GetForecastStatsQuery, DemandForecastStatsDto>
{
    private readonly MLSettings mlConfig = mlSettings.Value;

    public async Task<DemandForecastStatsDto> Handle(GetForecastStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting forecast stats. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-mlConfig.DefaultAnalysisPeriodDays);
        var end = request.EndDate ?? DateTime.UtcNow;

        var totalProducts = await context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive, cancellationToken);

        var productsWithSales = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt <= end)
            .Select(oi => oi.ProductId)
            .Distinct()
            .CountAsync(cancellationToken);

        logger.LogInformation("Forecast stats retrieved successfully.");

        return new DemandForecastStatsDto(
            totalProducts,
            productsWithSales,
            totalProducts > 0 ? (decimal)productsWithSales / totalProducts * 100 : 0,
            start,
            end
        );
    }
}
