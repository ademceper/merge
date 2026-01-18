namespace Merge.Application.DTOs.Seller;

public record OrderTrendDto
{
    public DateTime Date { get; init; }
    public int OrderCount { get; init; }
    public int CompletedOrders { get; init; }
    public int CancelledOrders { get; init; }
}
