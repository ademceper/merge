using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetLiveChatStats;

public class GetLiveChatStatsQueryHandler(IDbContext context, IOptions<SupportSettings> settings) : IRequestHandler<GetLiveChatStatsQuery, LiveChatStatsDto>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<LiveChatStatsDto> Handle(GetLiveChatStatsQuery request, CancellationToken cancellationToken)
    {
        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-supportConfig.DefaultStatsPeriodDays);
        var end = request.EndDate ?? DateTime.UtcNow;

        // Not: Şu anda sadece 1 Include var ama gelecekte ek Include'lar eklenebilir
        IQueryable<LiveChatSession> query = context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.Agent)
            .Where(s => s.CreatedAt >= start && s.CreatedAt <= end);

        var totalSessions = await query.CountAsync(cancellationToken);
        var activeSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Active, cancellationToken);
        var waitingSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Waiting, cancellationToken);
        var resolvedSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Resolved || s.Status == ChatSessionStatus.Closed, cancellationToken);

        var resolvedSessionsQuery = query.Where(s => (s.Status == ChatSessionStatus.Resolved || s.Status == ChatSessionStatus.Closed) && s.ResolvedAt.HasValue && s.StartedAt.HasValue);
        var avgResolutionTime = await resolvedSessionsQuery.AnyAsync(cancellationToken)
            ? await resolvedSessionsQuery
                .AverageAsync(s => (double)(s.ResolvedAt!.Value - s.StartedAt!.Value).TotalMinutes, cancellationToken)
            : 0;

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
