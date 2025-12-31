using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Support;

public class TicketStatsDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int TicketsToday { get; set; }
    public int TicketsThisWeek { get; set; }
    public int TicketsThisMonth { get; set; }
    public decimal AverageResponseTime { get; set; } // in hours
    public decimal AverageResolutionTime { get; set; } // in hours
    public Dictionary<string, int> TicketsByCategory { get; set; } = new();
    public Dictionary<string, int> TicketsByPriority { get; set; } = new();
}
