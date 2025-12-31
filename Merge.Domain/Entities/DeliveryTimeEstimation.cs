namespace Merge.Domain.Entities;

public class DeliveryTimeEstimation : BaseEntity
{
    public Guid? ProductId { get; set; } // Product-specific estimation
    public Guid? CategoryId { get; set; } // Category-based estimation
    public Guid? WarehouseId { get; set; } // Warehouse-specific estimation
    public Guid? ShippingProviderId { get; set; } // Provider-specific estimation
    public string? City { get; set; } // City-specific estimation
    public string? Country { get; set; } // Country-specific estimation
    public int MinDays { get; set; } // Minimum delivery days
    public int MaxDays { get; set; } // Maximum delivery days
    public int AverageDays { get; set; } // Average delivery days
    public bool IsActive { get; set; } = true;
    public string? Conditions { get; set; } // JSON for conditions (e.g., stock availability, order time)
    
    // Navigation properties
    public Product? Product { get; set; }
    public Category? Category { get; set; }
    public Warehouse? Warehouse { get; set; }
}

