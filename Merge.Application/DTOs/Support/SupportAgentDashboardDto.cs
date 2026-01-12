namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record SupportAgentDashboardDto(
    Guid AgentId,
    string AgentName,
    // Overview Stats
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int UnassignedTickets,
    // Performance Metrics
    decimal AverageResponseTime, // Hours
    decimal AverageResolutionTime, // Hours
    int TicketsResolvedToday,
    int TicketsResolvedThisWeek,
    int TicketsResolvedThisMonth,
    decimal ResolutionRate, // Percentage
    decimal CustomerSatisfactionScore, // 1-5
    // Workload Metrics
    int ActiveTickets,
    int OverdueTickets,
    int HighPriorityTickets,
    int UrgentTickets,
    // Category Breakdown
    IReadOnlyList<CategoryTicketCountDto> TicketsByCategory,
    // Priority Breakdown
    IReadOnlyList<PriorityTicketCountDto> TicketsByPriority,
    // Time-based Trends
    IReadOnlyList<TicketTrendDto> TicketTrends,
    // Recent Activity
    IReadOnlyList<SupportTicketDto> RecentTickets,
    IReadOnlyList<SupportTicketDto> UrgentTicketsList
);
