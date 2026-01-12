using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Support.Queries.GetTicketStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTicketStatsQueryHandler : IRequestHandler<GetTicketStatsQuery, TicketStatsDto>
{
    private readonly IDbContext _context;
    private readonly SupportSettings _settings;

    public GetTicketStatsQueryHandler(
        IDbContext context,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<TicketStatsDto> Handle(GetTicketStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
            .AsNoTracking();

        var now = DateTime.UtcNow;
        var today = now.Date;
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var weekAgo = now.AddDays(-_settings.WeeklyReportDays);
        var monthAgo = now.AddDays(-_settings.DefaultStatsPeriodDays);

        var totalTickets = await query.CountAsync(cancellationToken);
        var openTickets = await query.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var inProgressTickets = await query.CountAsync(t => t.Status == TicketStatus.InProgress, cancellationToken);
        var resolvedTickets = await query.CountAsync(t => t.Status == TicketStatus.Resolved, cancellationToken);
        var closedTickets = await query.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);
        var ticketsToday = await query.CountAsync(t => t.CreatedAt >= today, cancellationToken);
        var ticketsThisWeek = await query.CountAsync(t => t.CreatedAt >= weekAgo, cancellationToken);
        var ticketsThisMonth = await query.CountAsync(t => t.CreatedAt >= monthAgo, cancellationToken);

        // ✅ PERFORMANCE: Database'de average hesapla
        var resolvedTicketsQuery = query.Where(t => t.ResolvedAt.HasValue);
        var avgResolutionTime = await resolvedTicketsQuery.AnyAsync(cancellationToken)
            ? await resolvedTicketsQuery
                .AverageAsync(t => (double)(t.ResolvedAt!.Value - t.CreatedAt).TotalHours, cancellationToken)
            : 0;

        var ticketsWithResponseQuery = query.Where(t => t.LastResponseAt.HasValue && t.ResolvedAt.HasValue);
        var avgResponseTime = await ticketsWithResponseQuery.AnyAsync(cancellationToken)
            ? await ticketsWithResponseQuery
                .AverageAsync(t => (double)(t.LastResponseAt!.Value - t.CreatedAt).TotalHours, cancellationToken)
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap
        var ticketsByCategory = await query
            .GroupBy(t => t.Category.ToString())
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count, cancellationToken);

        var ticketsByPriority = await query
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count, cancellationToken);

        return new TicketStatsDto(
            totalTickets,
            openTickets,
            inProgressTickets,
            resolvedTickets,
            closedTickets,
            ticketsToday,
            ticketsThisWeek,
            ticketsThisMonth,
            (decimal)Math.Round(avgResponseTime, 2),
            (decimal)Math.Round(avgResolutionTime, 2),
            ticketsByCategory,
            ticketsByPriority
        );
    }
}
