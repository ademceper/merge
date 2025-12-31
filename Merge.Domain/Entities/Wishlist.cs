namespace Merge.Domain.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

