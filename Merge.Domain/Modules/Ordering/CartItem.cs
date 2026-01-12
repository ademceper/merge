using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// CartItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class CartItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; } // Seçilen varyant (renk, beden vb.)
    public int Quantity { get; private set; }
    public decimal Price { get; private set; } // Sepete eklendiğindeki fiyat

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public ProductVariant? ProductVariant { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CartItem() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static CartItem Create(Guid cartId, Guid productId, int quantity, decimal price, Guid? productVariantId = null)
    {
        Guard.AgainstDefault(cartId, nameof(cartId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNegativeOrZero(price, nameof(price));

        return new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            Quantity = quantity,
            Price = price,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Logic - Update quantity
    public void UpdateQuantity(int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

