using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetAnalyticsSummary;

public class GetAnalyticsSummaryQueryHandler(
    IDbContext context,
    ILogger<GetAnalyticsSummaryQueryHandler> logger,
    IOptions<ServiceSettings> serviceSettings) : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummaryDto>
{

    public async Task<AnalyticsSummaryDto> Handle(GetAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching analytics summary. Days: {Days}", request.Days);
        
        var days = request.Days == 30 ? serviceSettings.Value.DefaultDateRangeDays : request.Days;
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var summary = new AnalyticsSummaryDto(
            Period: $"Last {days} days",
            NewUsers: await context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= startDate, cancellationToken),
            NewOrders: await context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.CreatedAt >= startDate, cancellationToken),
            Revenue: await context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt >= startDate)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            AverageOrderValue: await context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDate)
                .AverageAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0,
            NewProducts: await context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.CreatedAt >= startDate, cancellationToken),
            TotalReviews: await context.Set<ReviewEntity>().AsNoTracking().CountAsync(r => r.CreatedAt >= startDate, cancellationToken),
            AverageRating: await context.Set<ReviewEntity>()
                .AsNoTracking()
                .Where(r => r.CreatedAt >= startDate)
                .AverageAsync(r => (decimal?)r.Rating, cancellationToken) ?? 0
        );

        logger.LogInformation("Analytics summary calculated. Days: {Days}, Revenue: {Revenue}, NewUsers: {NewUsers}",
            days, summary.Revenue, summary.NewUsers);

        return summary;
    }
}

