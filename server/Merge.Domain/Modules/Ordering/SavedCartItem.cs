using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;


public class SavedCartItem : BaseEntity
{
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
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    [NotMapped]
    public Money PriceMoney => new Money(_price);

    private SavedCartItem() { }

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

    public void UpdateQuantity(int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));
        _quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Money newPrice)
    {
        Guard.AgainstNull(newPrice, nameof(newPrice));
        Guard.AgainstNegativeOrZero(newPrice.Amount, nameof(newPrice));
        _price = newPrice.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

