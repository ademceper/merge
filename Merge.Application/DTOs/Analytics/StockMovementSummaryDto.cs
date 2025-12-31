namespace Merge.Application.DTOs.Analytics;

public class StockMovementSummaryDto
{
    public string MovementType { get; set; } = string.Empty;
    public int Count { get; set; }
    public int TotalQuantity { get; set; }
    public DateTime Date { get; set; }
}
