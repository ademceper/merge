using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; } // Seçilen varyant (renk, beden vb.)
    public int Quantity { get; set; }
    public decimal Price { get; set; } // Sepete eklendiğindeki fiyat

    // ✅ CONCURRENCY: Eşzamanlı sepet güncellemelerini önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductVariant? ProductVariant { get; set; }
}

