namespace Merge.Application.DTOs.Cart;

public class AbandonedCartRecoveryStatsDto
{
    public int TotalAbandonedCarts { get; set; }
    public decimal TotalAbandonedValue { get; set; }
    public int EmailsSent { get; set; }
    public int EmailsOpened { get; set; }
    public int EmailsClicked { get; set; }
    public int RecoveredCarts { get; set; }
    public decimal RecoveredRevenue { get; set; }
    public decimal RecoveryRate { get; set; }
    public decimal AverageCartValue { get; set; }
}
