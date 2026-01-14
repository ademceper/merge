namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record TicketStatsDto(
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int TicketsToday,
    int TicketsThisWeek,
    int TicketsThisMonth,
    decimal AverageResponseTime, // in hours
    decimal AverageResolutionTime, // in hours
    IReadOnlyDictionary<string, int> TicketsByCategory,
    IReadOnlyDictionary<string, int> TicketsByPriority
);
