namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Available Stock DTO - BOLUM 4.3: Over-Posting Koruması (Anonymous object YASAK)
/// </summary>
public class AvailableStockDto
{
    public Guid ProductId { get; set; }
    public Guid? WarehouseId { get; set; }
    public int AvailableStock { get; set; }
}

