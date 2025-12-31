namespace Merge.Application.DTOs.Support;

public class SupportAgentDashboardDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    
    // Overview Stats
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int UnassignedTickets { get; set; }
    
    // Performance Metrics
    public decimal AverageResponseTime { get; set; } // Hours
    public decimal AverageResolutionTime { get; set; } // Hours
    public int TicketsResolvedToday { get; set; }
    public int TicketsResolvedThisWeek { get; set; }
    public int TicketsResolvedThisMonth { get; set; }
    public decimal ResolutionRate { get; set; } // Percentage
    public decimal CustomerSatisfactionScore { get; set; } // 1-5
    
    // Workload Metrics
    public int ActiveTickets { get; set; }
    public int OverdueTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int UrgentTickets { get; set; }
    
    // Category Breakdown
    public List<CategoryTicketCountDto> TicketsByCategory { get; set; } = new();
    
    // Priority Breakdown
    public List<PriorityTicketCountDto> TicketsByPriority { get; set; } = new();
    
    // Time-based Trends
    public List<TicketTrendDto> TicketTrends { get; set; } = new();
    
    // Recent Activity
    public List<SupportTicketDto> RecentTickets { get; set; } = new();
    public List<SupportTicketDto> UrgentTicketsList { get; set; } = new();
}
