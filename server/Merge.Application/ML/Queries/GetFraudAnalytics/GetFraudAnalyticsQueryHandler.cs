using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.GetFraudAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetFraudAnalyticsQueryHandler(IDbContext context, ILogger<GetFraudAnalyticsQueryHandler> logger, IOptions<MLSettings> mlSettings) : IRequestHandler<GetFraudAnalyticsQuery, FraudAnalyticsDto>
{
    private readonly MLSettings config = mlSettings.Value;

    public async Task<FraudAnalyticsDto> Handle(GetFraudAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting fraud analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        // DefaultAnalysisPeriodDays kullan (30 gün = yaklaşık 1 ay)
        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-config.DefaultAnalysisPeriodDays);
        var end = request.EndDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalAlerts = await context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end, cancellationToken);

        var pendingAlerts = await context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.Pending, cancellationToken);

        var resolvedAlerts = await context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.Resolved, cancellationToken);

        var falsePositiveAlerts = await context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.FalsePositive, cancellationToken);

        var avgRiskScore = totalAlerts > 0
            ? await context.Set<FraudAlert>()
                .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
                .AverageAsync(a => (decimal?)a.RiskScore, cancellationToken) ?? 0
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var alertsByType = await context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.AlertType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type.ToString(), x => x.Count, cancellationToken);

        // ✅ BOLUM 1.2: Enum kullanımı - Dictionary için string'e çevir (DTO uyumluluğu için)
        var alertsByStatus = await context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de filtreleme ve sıralama yap (memory'de işlem YASAK)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var highRiskAlerts = await context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.RiskScore >= config.FraudDetectionHighRiskThreshold)
            .OrderByDescending(a => a.RiskScore)
            .Take(config.FraudDetectionHighRiskAlertsLimit)
            .Select(a => new HighRiskAlertDto(
                a.Id,
                a.AlertType.ToString(), // ✅ BOLUM 1.2: Enum -> string (DTO uyumluluğu)
                a.RiskScore,
                a.Status.ToString(), // ✅ BOLUM 1.2: Enum -> string (DTO uyumluluğu)
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Fraud analytics retrieved successfully.");

        return new FraudAnalyticsDto(
            totalAlerts,
            pendingAlerts,
            resolvedAlerts,
            falsePositiveAlerts,
            (decimal)Math.Round(avgRiskScore, 2),
            alertsByType,
            alertsByStatus,
            highRiskAlerts
        );
    }
}
