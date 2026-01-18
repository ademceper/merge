using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetAgentDashboard;

public class GetAgentDashboardQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAgentDashboardQueryHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<GetAgentDashboardQuery, SupportAgentDashboardDto>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<SupportAgentDashboardDto> Handle(GetAgentDashboardQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-supportConfig.DefaultStatsPeriodDays);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        logger.LogInformation("Generating agent dashboard for agent {AgentId} from {StartDate} to {EndDate}",
            request.AgentId, startDate, endDate);

        var agent = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.AgentId, cancellationToken);

        if (agent is null)
        {
            logger.LogWarning("Agent {AgentId} not found", request.AgentId);
            throw new NotFoundException("Ajan", request.AgentId);
        }

        IQueryable<SupportTicket> allTicketsQuery = context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToId == request.AgentId);

        var totalTickets = await allTicketsQuery.CountAsync(cancellationToken);
        var openTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var inProgressTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.InProgress, cancellationToken);
        var resolvedTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Resolved, cancellationToken);
        var closedTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);

        // Unassigned tickets (for admin view)
        var unassignedTickets = await context.Set<SupportTicket>()
            .AsNoTracking()
            .CountAsync(t => t.AssignedToId == null && t.Status != TicketStatus.Closed, cancellationToken);

        var resolvedTicketsQuery = allTicketsQuery.Where(t => t.ResolvedAt.HasValue);
        var averageResolutionTime = await resolvedTicketsQuery.AnyAsync(cancellationToken)
            ? await resolvedTicketsQuery
                .AverageAsync(t => (double)(t.ResolvedAt!.Value - t.CreatedAt).TotalHours, cancellationToken)
            : 0;

        var ticketsWithResponseQuery = allTicketsQuery.Where(t => t.LastResponseAt.HasValue);
        var averageResponseTime = await ticketsWithResponseQuery.AnyAsync(cancellationToken)
            ? await ticketsWithResponseQuery
                .AverageAsync(t => (double)(t.LastResponseAt!.Value - t.CreatedAt).TotalHours, cancellationToken)
            : 0;

        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-supportConfig.WeeklyReportDays);
        var monthAgo = today.AddDays(-supportConfig.DefaultStatsPeriodDays);

        var ticketsResolvedToday = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == today, cancellationToken);
        var ticketsResolvedThisWeek = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value >= weekAgo, cancellationToken);
        var ticketsResolvedThisMonth = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value >= monthAgo, cancellationToken);

        var resolutionRate = totalTickets > 0
            ? (decimal)(resolvedTickets + closedTickets) / totalTickets * 100
            : 0;

        var activeTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress, cancellationToken);
        var overdueTickets = await allTicketsQuery.CountAsync(t =>
            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress) &&
            t.CreatedAt < DateTime.UtcNow.AddDays(-supportConfig.TicketOverdueDays), cancellationToken);
        var highPriorityTickets = await allTicketsQuery.CountAsync(t => t.Priority == TicketPriority.High, cancellationToken);
        var urgentTickets = await allTicketsQuery.CountAsync(t => t.Priority == TicketPriority.Urgent, cancellationToken);

        // Category breakdown
        var ticketsByCategory = await GetTicketsByCategoryAsync(request.AgentId, startDate, endDate, cancellationToken);

        // Priority breakdown
        var ticketsByPriority = await GetTicketsByPriorityAsync(request.AgentId, startDate, endDate, cancellationToken);

        // Trends
        var trends = await GetTicketTrendsAsync(request.AgentId, startDate, endDate, cancellationToken);

        var recentTickets = await allTicketsQuery
            .AsSplitQuery()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .Take(supportConfig.DashboardRecentTicketsCount)
            .ToListAsync(cancellationToken);

        var urgentTicketsList = await allTicketsQuery
            .AsSplitQuery()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => t.Priority == TicketPriority.Urgent && (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
            .OrderBy(t => t.CreatedAt)
            .Take(supportConfig.DashboardUrgentTicketsCount)
            .ToListAsync(cancellationToken);

        var recentTicketIds = await allTicketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Take(supportConfig.DashboardRecentTicketsCount)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
        
        var urgentTicketIds = await allTicketsQuery
            .Where(t => t.Priority == TicketPriority.Urgent && (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
            .OrderBy(t => t.CreatedAt)
            .Take(supportConfig.DashboardUrgentTicketsCount)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
        
        var allTicketIds = recentTicketIds.Concat(urgentTicketIds).Distinct().ToList();
        var messagesDict = await context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => allTicketIds.Contains(m.TicketId))
            .GroupBy(m => m.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages, cancellationToken);

        var attachmentsDict = await context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => allTicketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments, cancellationToken);

        List<SupportTicketDto> recentTicketsDto = [];
        foreach (var ticket in recentTickets)
        {
            var dto = mapper.Map<SupportTicketDto>(ticket);
            
            IReadOnlyList<TicketMessageDto> messages;
            if (messagesDict.TryGetValue(ticket.Id, out var messageList))
            {
                messages = mapper.Map<List<TicketMessageDto>>(messageList).AsReadOnly();
            }
            else
            {
                messages = Array.Empty<TicketMessageDto>().AsReadOnly();
            }
            
            IReadOnlyList<TicketAttachmentDto> attachments;
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachmentList))
            {
                attachments = mapper.Map<List<TicketAttachmentDto>>(attachmentList).AsReadOnly();
            }
            else
            {
                attachments = Array.Empty<TicketAttachmentDto>().AsReadOnly();
            }
            
            recentTicketsDto.Add(dto with { Messages = messages, Attachments = attachments });
        }

        List<SupportTicketDto> urgentTicketsDto = [];
        foreach (var ticket in urgentTicketsList)
        {
            var dto = mapper.Map<SupportTicketDto>(ticket);
            
            IReadOnlyList<TicketMessageDto> messages;
            if (messagesDict.TryGetValue(ticket.Id, out var messageList))
            {
                messages = mapper.Map<List<TicketMessageDto>>(messageList).AsReadOnly();
            }
            else
            {
                messages = Array.Empty<TicketMessageDto>().AsReadOnly();
            }
            
            IReadOnlyList<TicketAttachmentDto> attachments;
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachmentList))
            {
                attachments = mapper.Map<List<TicketAttachmentDto>>(attachmentList).AsReadOnly();
            }
            else
            {
                attachments = Array.Empty<TicketAttachmentDto>().AsReadOnly();
            }
            
            urgentTicketsDto.Add(dto with { Messages = messages, Attachments = attachments });
        }

        logger.LogInformation("Agent dashboard generated for {AgentName}. Total tickets: {Total}, Active: {Active}, Resolution rate: {Rate}%",
            $"{agent.FirstName} {agent.LastName}", totalTickets, activeTickets, Math.Round(resolutionRate, 2));

        return new SupportAgentDashboardDto(
            request.AgentId,
            $"{agent.FirstName} {agent.LastName}",
            totalTickets,
            openTickets,
            inProgressTickets,
            resolvedTickets,
            closedTickets,
            unassignedTickets,
            Math.Round((decimal)averageResponseTime, 2),
            Math.Round((decimal)averageResolutionTime, 2),
            ticketsResolvedToday,
            ticketsResolvedThisWeek,
            ticketsResolvedThisMonth,
            Math.Round(resolutionRate, 2),
            0, // CustomerSatisfactionScore - Would need feedback system
            activeTickets,
            overdueTickets,
            highPriorityTickets,
            urgentTickets,
            ticketsByCategory.AsReadOnly(),
            ticketsByPriority.AsReadOnly(),
            trends.AsReadOnly(),
            recentTicketsDto.AsReadOnly(),
            urgentTicketsDto.AsReadOnly()
        );
    }

    private async Task<List<CategoryTicketCountDto>> GetTicketsByCategoryAsync(Guid? agentId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        IQueryable<SupportTicket> query = context.Set<SupportTicket>()
            .AsNoTracking();

        if (agentId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == agentId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var grouped = await query
            .GroupBy(t => t.Category.ToString())
            .Select(g => new CategoryTicketCountDto(
                g.Key,
                g.Count(),
                total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            ))
            .OrderByDescending(c => c.Count)
            .ToListAsync(cancellationToken);

        return grouped;
    }

    private async Task<List<PriorityTicketCountDto>> GetTicketsByPriorityAsync(Guid? agentId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        IQueryable<SupportTicket> query = context.Set<SupportTicket>()
            .AsNoTracking();

        if (agentId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == agentId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var grouped = await query
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new PriorityTicketCountDto(
                g.Key,
                g.Count(),
                total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            ))
            .OrderByDescending(p => p.Count)
            .ToListAsync(cancellationToken);

        return grouped;
    }

    private async Task<List<TicketTrendDto>> GetTicketTrendsAsync(Guid? agentId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        IQueryable<SupportTicket> query = context.Set<SupportTicket>()
            .AsNoTracking();

        if (agentId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == agentId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var trends = await query
            .GroupBy(t => new { Year = t.CreatedAt.Year, Month = t.CreatedAt.Month, Day = t.CreatedAt.Day })
            .Select(g => new TicketTrendDto(
                new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                g.Count(),
                g.Count(t => t.ResolvedAt.HasValue && 
                           t.ResolvedAt.Value.Year == g.Key.Year &&
                           t.ResolvedAt.Value.Month == g.Key.Month &&
                           t.ResolvedAt.Value.Day == g.Key.Day),
                g.Count(t => t.ClosedAt.HasValue &&
                             t.ClosedAt.Value.Year == g.Key.Year &&
                             t.ClosedAt.Value.Month == g.Key.Month &&
                             t.ClosedAt.Value.Day == g.Key.Day)
            ))
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);

        return trends;
    }
}
