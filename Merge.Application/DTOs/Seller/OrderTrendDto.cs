namespace Merge.Application.DTOs.Seller;

public class OrderTrendDto
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
}
