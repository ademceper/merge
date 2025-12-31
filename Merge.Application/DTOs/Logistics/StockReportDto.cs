using Merge.Domain.Entities;
namespace Merge.Application.DTOs.Logistics;

public class StockReportDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int TotalReserved { get; set; }
    public int TotalAvailable { get; set; }
    public decimal TotalValue { get; set; }
    public List<InventoryDto> WarehouseBreakdown { get; set; } = new();
}
