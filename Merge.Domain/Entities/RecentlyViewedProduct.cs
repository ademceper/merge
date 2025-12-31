namespace Merge.Domain.Entities;

public class RecentlyViewedProduct : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

