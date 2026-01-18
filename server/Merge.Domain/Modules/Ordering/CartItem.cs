using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;


public class CartItem : BaseEntity
{
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

    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public ProductVariant? ProductVariant { get; private set; }

    [NotMapped]
    public Money PriceMoney => new Money(_price);

    private CartItem() { }

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

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

