namespace Merge.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Unique warehouse code (WH001, WH002, etc.)
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public int Capacity { get; set; } = 0; // Total capacity in square meters or units
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
