using Merge.Application.DTOs.Logistics;

namespace Merge.Application.DTOs.Analytics;

public class InventoryOverviewDto
{
    public int TotalWarehouses { get; set; }
    public int TotalInventoryItems { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int LowStockCount { get; set; }
    public IEnumerable<LowStockAlertDto> LowStockAlerts { get; set; } = new List<LowStockAlertDto>();
    public int TotalStockQuantity { get; set; }
    public int ReservedStockQuantity { get; set; }
}
