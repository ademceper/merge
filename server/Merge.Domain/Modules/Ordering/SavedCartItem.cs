using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// SavedCartItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class SavedCartItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    
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
    
    public string? Notes { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // ✅ BOLUM 1.3: Value Object property (computed from decimal)
    [NotMapped]
    public Money PriceMoney => new Money(_price);

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SavedCartItem() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
    public static SavedCartItem Create(Guid userId, Guid productId, int quantity, Money price, string? notes = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNull(price, nameof(price));
        Guard.AgainstNegativeOrZero(price.Amount, nameof(price));

        return new SavedCartItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            _quantity = quantity, // EF Core compatibility - backing field
            _price = price.Amount, // EF Core compatibility - backing field
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Logic - Update quantity
    public void UpdateQuantity(int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));
        _quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update price
    // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
    public void UpdatePrice(Money newPrice)
    {
        Guard.AgainstNull(newPrice, nameof(newPrice));
        Guard.AgainstNegativeOrZero(newPrice.Amount, nameof(newPrice));
        _price = newPrice.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update notes
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

