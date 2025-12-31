namespace Merge.Application.DTOs.Logistics;

public class PickPackDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string PackNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? PickedByUserId { get; set; }
    public string? PickedByName { get; set; }
    public Guid? PackedByUserId { get; set; }
    public string? PackedByName { get; set; }
    public DateTime? PickedAt { get; set; }
    public DateTime? PackedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? Notes { get; set; }
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; }
    public int PackageCount { get; set; }
    public List<PickPackItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
