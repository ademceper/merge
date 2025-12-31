namespace Merge.Domain.Entities;

public class BundleItem : BaseEntity
{
    public Guid BundleId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public ProductBundle Bundle { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

