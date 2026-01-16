namespace Merge.Application.DTOs.Catalog;

/// <summary>
/// Partial update DTO for Inventory (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchInventoryDto
{
    public int? MinimumStockLevel { get; init; }
    public int? MaximumStockLevel { get; init; }
    public decimal? UnitCost { get; init; }
    public string? Location { get; init; }
}
