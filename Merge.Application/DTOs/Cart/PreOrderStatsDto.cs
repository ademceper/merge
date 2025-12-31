namespace Merge.Application.DTOs.Cart;

public class PreOrderStatsDto
{
    public int TotalPreOrders { get; set; }
    public int PendingPreOrders { get; set; }
    public int ConfirmedPreOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDeposits { get; set; }
    public List<PreOrderDto> RecentPreOrders { get; set; } = new();
}
