using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.GetAuditStats;

public class GetAuditStatsQueryHandler(
    IDbContext context,
    ILogger<GetAuditStatsQueryHandler> logger) : IRequestHandler<GetAuditStatsQuery, AuditStatsDto>
{

    public async Task<AuditStatsDto> Handle(GetAuditStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving audit stats. Days: {Days}", request.Days);

        var startDate = DateTime.UtcNow.AddDays(-request.Days);
        var today = DateTime.UtcNow.Date;

        var query = context.Set<AuditLog>()
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

        var mostActiveUsers = await context.Set<AuditLog>()
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

        var recentCriticalEvents = await context.Set<AuditLog>()
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
