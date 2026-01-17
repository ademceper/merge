using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Queries.GetSecuritySummary;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSecuritySummaryQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSecuritySummaryQueryHandler> logger, IOptions<ServiceSettings> serviceSettings, IOptions<SecuritySettings> securitySettings) : IRequestHandler<GetSecuritySummaryQuery, SecurityMonitoringSummaryDto>
{
    private readonly ServiceSettings serviceConfig = serviceSettings.Value;
    private readonly SecuritySettings securityConfig = securitySettings.Value;

    public async Task<SecurityMonitoringSummaryDto> Handle(GetSecuritySummaryQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-serviceConfig.DefaultDateRangeDays);
        var end = request.EndDate ?? DateTime.UtcNow;
        
        logger.LogInformation("Security summary sorgulanıyor. StartDate: {StartDate}, EndDate: {EndDate}", start, end);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted and !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalEvents = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .CountAsync(cancellationToken);

        var suspiciousEvents = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end && e.IsSuspicious)
            .CountAsync(cancellationToken);

        var criticalEvents = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end && e.Severity == SecurityEventSeverity.Critical)
            .CountAsync(cancellationToken);

        var pendingAlerts = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == AlertStatus.New)
            .CountAsync(cancellationToken);

        var resolvedAlerts = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == AlertStatus.Resolved)
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var eventsByType = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .GroupBy(e => e.EventType)
            .Select(g => new { EventType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventType.ToString(), x => x.Count, cancellationToken);

        var alertsBySeverity = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity.ToString(), x => x.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de filtreleme/sıralama yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için Cartesian Explosion önleme
        // ✅ BOLUM 12.0: Magic number config'den
        var recentCriticalAlerts = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end &&
                       a.Severity == AlertSeverity.Critical && a.Status != AlertStatus.Resolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(securityConfig.RecentCriticalAlertsLimit)
            .ToListAsync(cancellationToken);

        var recentCriticalAlertsDtos = mapper.Map<IEnumerable<SecurityAlertDto>>(recentCriticalAlerts).ToList();

        logger.LogInformation("Security summary oluşturuldu. TotalEvents: {TotalEvents}, SuspiciousEvents: {SuspiciousEvents}, CriticalEvents: {CriticalEvents}, PendingAlerts: {PendingAlerts}, ResolvedAlerts: {ResolvedAlerts}",
            totalEvents, suspiciousEvents, criticalEvents, pendingAlerts, resolvedAlerts);

        return new SecurityMonitoringSummaryDto
        {
            TotalSecurityEvents = totalEvents,
            SuspiciousEvents = suspiciousEvents,
            CriticalEvents = criticalEvents,
            PendingAlerts = pendingAlerts,
            ResolvedAlerts = resolvedAlerts,
            EventsByType = eventsByType,
            AlertsBySeverity = alertsBySeverity,
            RecentCriticalAlerts = recentCriticalAlertsDtos
        };
    }
}
