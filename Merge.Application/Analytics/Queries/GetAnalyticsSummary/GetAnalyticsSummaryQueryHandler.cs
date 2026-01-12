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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAnalyticsSummaryQueryHandler : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummaryDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetAnalyticsSummaryQueryHandler> _logger;
    private readonly ServiceSettings _serviceSettings;

    public GetAnalyticsSummaryQueryHandler(
        IDbContext context,
        ILogger<GetAnalyticsSummaryQueryHandler> logger,
        IOptions<ServiceSettings> serviceSettings)
    {
        _context = context;
        _logger = logger;
        _serviceSettings = serviceSettings.Value;
    }

    public async Task<AnalyticsSummaryDto> Handle(GetAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching analytics summary. Days: {Days}", request.Days);
        
        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var days = request.Days == 30 ? _serviceSettings.DefaultDateRangeDays : request.Days;
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        // ✅ PERFORMANCE: Removed manual !IsDeleted checks (Global Query Filter handles it)
        var summary = new AnalyticsSummaryDto(
            Period: $"Last {days} days",
            NewUsers: await _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= startDate, cancellationToken),
            NewOrders: await _context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.CreatedAt >= startDate, cancellationToken),
            Revenue: await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt >= startDate)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            AverageOrderValue: await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDate)
                .AverageAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0,
            NewProducts: await _context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.CreatedAt >= startDate, cancellationToken),
            TotalReviews: await _context.Set<ReviewEntity>().AsNoTracking().CountAsync(r => r.CreatedAt >= startDate, cancellationToken),
            AverageRating: await _context.Set<ReviewEntity>()
                .AsNoTracking()
                .Where(r => r.CreatedAt >= startDate)
                .AverageAsync(r => (decimal?)r.Rating, cancellationToken) ?? 0
        );

        _logger.LogInformation("Analytics summary calculated. Days: {Days}, Revenue: {Revenue}, NewUsers: {NewUsers}",
            days, summary.Revenue, summary.NewUsers);

        return summary;
    }
}

