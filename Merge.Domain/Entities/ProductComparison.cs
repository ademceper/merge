namespace Merge.Domain.Entities;

public class ProductComparison : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty; // Optional name for saved comparison
    public bool IsSaved { get; set; } = false;
    public string? ShareCode { get; set; } // For sharing comparisons
    public ICollection<ProductComparisonItem> Items { get; set; } = new List<ProductComparisonItem>();
}

public class ProductComparisonItem : BaseEntity
{
    public Guid ComparisonId { get; set; }
    public ProductComparison Comparison { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Position { get; set; } = 0; // Order in comparison
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
