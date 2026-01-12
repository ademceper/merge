using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Support.Queries.GetLiveChatStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetLiveChatStatsQueryHandler : IRequestHandler<GetLiveChatStatsQuery, LiveChatStatsDto>
{
    private readonly IDbContext _context;
    private readonly SupportSettings _settings;

    public GetLiveChatStatsQueryHandler(
        IDbContext context,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<LiveChatStatsDto> Handle(GetLiveChatStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-_settings.DefaultStatsPeriodDays);
        var end = request.EndDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        // Not: Şu anda sadece 1 Include var ama gelecekte ek Include'lar eklenebilir
        IQueryable<LiveChatSession> query = _context.Set<LiveChatSession>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.Agent)
            .Where(s => s.CreatedAt >= start && s.CreatedAt <= end);

        var totalSessions = await query.CountAsync(cancellationToken);
        var activeSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Active, cancellationToken);
        var waitingSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Waiting, cancellationToken);
        var resolvedSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Resolved || s.Status == ChatSessionStatus.Closed, cancellationToken);

        // ✅ PERFORMANCE: Database'de average hesapla
        var resolvedSessionsQuery = query.Where(s => (s.Status == ChatSessionStatus.Resolved || s.Status == ChatSessionStatus.Closed) && s.ResolvedAt.HasValue && s.StartedAt.HasValue);
        var avgResolutionTime = await resolvedSessionsQuery.AnyAsync(cancellationToken)
            ? await resolvedSessionsQuery
                .AverageAsync(s => (double)(s.ResolvedAt!.Value - s.StartedAt!.Value).TotalMinutes, cancellationToken)
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap
        var sessionsByDepartment = await query
            .Where(s => !string.IsNullOrEmpty(s.Department))
            .GroupBy(s => s.Department!)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Department, x => x.Count, cancellationToken);

        var sessionsByAgent = await query
            .Where(s => s.AgentId.HasValue)
            .GroupBy(s => s.Agent != null ? $"{s.Agent.FirstName} {s.Agent.LastName}" : "Unknown")
            .Select(g => new { AgentName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AgentName, x => x.Count, cancellationToken);

        return new LiveChatStatsDto(
            totalSessions,
            activeSessions,
            waitingSessions,
            resolvedSessions,
            0, // AverageResponseTime - Can be calculated from first agent message time
            (decimal)Math.Round(avgResolutionTime, 2), // AverageResolutionTime
            sessionsByDepartment,
            sessionsByAgent
        );
    }
}
