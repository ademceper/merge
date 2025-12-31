namespace Merge.Domain.Entities;

public class SavedCartItem : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; } // Kaydedildiği andaki fiyat
    public string? Notes { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

