using System.Linq;

namespace Merge.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    
    public decimal TotalAmount => CartItems.Where(item => !item.IsDeleted).Sum(item => item.Quantity * item.Price);
}

