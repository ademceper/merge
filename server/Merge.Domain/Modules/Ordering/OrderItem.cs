using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// OrderItem Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// OrderItem, Order aggregate root'unun bir parçasıdır
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
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
    
    private decimal _unitPrice;
    private decimal _totalPrice;
    
    // Database columns (EF Core mapping)
    public decimal UnitPrice 
    { 
        get => _unitPrice; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(UnitPrice));
            _unitPrice = value;
        }
    }
    
    public decimal TotalPrice 
    { 
        get => _totalPrice; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(TotalPrice));
            _totalPrice = value;
        }
    }
    
    [NotMapped]
    public Money UnitPriceMoney => new Money(_unitPrice);
    
    [NotMapped]
    public Money TotalPriceMoney => new Money(_totalPrice);
    
    // Navigation properties
    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private OrderItem() { }

    
    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        Product product,
        int quantity,
        Money unitPrice)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNull(unitPrice, nameof(unitPrice));
        Guard.AgainstNegative(unitPrice.Amount, nameof(unitPrice));

        var totalPrice = new Money(unitPrice.Amount * quantity);

        if (totalPrice.Amount != unitPrice.Amount * quantity)
            throw new DomainException("Toplam fiyat hesaplama hatası");

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Product = product,
            _quantity = quantity, // EF Core compatibility - backing field
            _unitPrice = unitPrice.Amount, // EF Core compatibility - backing field
            _totalPrice = totalPrice.Amount, // EF Core compatibility - backing field
            CreatedAt = DateTime.UtcNow
        };

        return orderItem;
    }

    /// <summary>
    /// Miktarı günceller ve toplam fiyatı yeniden hesaplar
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));

        _quantity = newQuantity;
        _totalPrice = _unitPrice * _quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Birim fiyatı günceller ve toplam fiyatı yeniden hesaplar
    /// </summary>
    public void UpdateUnitPrice(Money newUnitPrice)
    {
        Guard.AgainstNull(newUnitPrice, nameof(newUnitPrice));
        Guard.AgainstNegative(newUnitPrice.Amount, nameof(newUnitPrice));

        _unitPrice = newUnitPrice.Amount;
        _totalPrice = _unitPrice * _quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}

