using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Governance.Queries.GetAuditStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAuditStatsQueryHandler : IRequestHandler<GetAuditStatsQuery, AuditStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetAuditStatsQueryHandler> _logger;

    public GetAuditStatsQueryHandler(
        IDbContext context,
        ILogger<GetAuditStatsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AuditStatsDto> Handle(GetAuditStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving audit stats. Days: {Days}", request.Days);

        var startDate = DateTime.UtcNow.AddDays(-request.Days);
        var today = DateTime.UtcNow.Date;

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var query = _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate);

        var totalAudits = await query.CountAsync(cancellationToken);
        var todayAudits = await query.CountAsync(a => a.CreatedAt.Date == today, cancellationToken);
        var failedActions = await query.CountAsync(a => !a.IsSuccessful, cancellationToken);
        var criticalEvents = await query.CountAsync(a => a.Severity == AuditSeverity.Critical, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap
        var actionsByType = await query
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Action, g => g.Count, cancellationToken);

        var actionsByModule = await query
            .Where(a => !string.IsNullOrEmpty(a.Module))
            .GroupBy(a => a.Module)
            .Select(g => new { Module = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Module!, g => g.Count, cancellationToken);

        var actionsBySeverity = await query
            .GroupBy(a => a.Severity.ToString())
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Severity, g => g.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap
        var mostActiveUsers = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate && a.UserId.HasValue)
            .Include(a => a.User)
            .GroupBy(a => a.UserId)
            .Select(g => new TopAuditUserDto
            {
                UserId = g.Key!.Value,
                UserEmail = g.First().UserEmail,
                ActionCount = g.Count(),
                LastAction = g.Max(a => a.CreatedAt)
            })
            .OrderByDescending(u => u.ActionCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de filtering yap
        var recentCriticalEvents = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate &&
                       a.Severity == AuditSeverity.Critical)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new RecentCriticalEventDto
            {
                Id = a.Id,
                Action = a.Action,
                UserEmail = a.UserEmail,
                EntityType = a.EntityType,
                CreatedAt = a.CreatedAt,
                ErrorMessage = a.ErrorMessage
            })
            .ToListAsync(cancellationToken);

        return new AuditStatsDto
        {
            TotalAudits = totalAudits,
            TodayAudits = todayAudits,
            FailedActions = failedActions,
            CriticalEvents = criticalEvents,
            ActionsByType = actionsByType,
            ActionsByModule = actionsByModule,
            ActionsBySeverity = actionsBySeverity,
            MostActiveUsers = mostActiveUsers,
            RecentCriticalEvents = recentCriticalEvents
        };
    }
}

