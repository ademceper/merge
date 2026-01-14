using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// CartItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class CartItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; } // Seçilen varyant (renk, beden vb.)
    
    private int _quantity;
    public int Quantity 
    { 
        get => _quantity; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Quantity));
            _quantity = value;
        }
    }
    
    // ✅ BOLUM 1.3: Value Objects - Money backing field (EF Core compatibility)
    private decimal _price;
    public decimal Price
    {
        get => _price;
        private set
        {
            Guard.AgainstNegativeOrZero(value, nameof(Price));
            _price = value;
        }
    }

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public ProductVariant? ProductVariant { get; private set; }

    // ✅ BOLUM 1.3: Value Object property (computed from decimal)
    [NotMapped]
    public Money PriceMoney => new Money(_price);

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CartItem() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
    public static CartItem Create(Guid cartId, Guid productId, int quantity, Money price, Guid? productVariantId = null)
    {
        Guard.AgainstDefault(cartId, nameof(cartId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNull(price, nameof(price));
        Guard.AgainstNegativeOrZero(price.Amount, nameof(price));

        return new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            _quantity = quantity, // EF Core compatibility - backing field
            _price = price.Amount, // EF Core compatibility - backing field
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Logic - Update quantity with maximum limit validation
    public void UpdateQuantity(int newQuantity, int? maxQuantity = null)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));
        
        if (maxQuantity.HasValue && newQuantity > maxQuantity.Value)
        {
            throw new DomainException($"Miktar maksimum {maxQuantity.Value} olabilir.");
        }

        _quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

