using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// OrderItem Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// OrderItem, Order aggregate root'unun bir parçasıdır
/// </summary>
public class OrderItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    
    // Navigation properties
    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private OrderItem() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    /// <summary>
    /// OrderItem oluşturur - BOLUM 1.1: Factory Method (ZORUNLU)
    /// </summary>
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
        Guard.AgainstNegative(unitPrice.Amount, nameof(unitPrice));

        var totalPrice = new Money(unitPrice.Amount * quantity);

        // ✅ BOLUM 1.6: Invariant validation - TotalPrice = UnitPrice * Quantity
        if (totalPrice.Amount != unitPrice.Amount * quantity)
            throw new DomainException("Toplam fiyat hesaplama hatası");

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Product = product,
            Quantity = quantity,
            UnitPrice = unitPrice.Amount, // EF Core compatibility
            TotalPrice = totalPrice.Amount,
            CreatedAt = DateTime.UtcNow
        };

        return orderItem;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update quantity (recalculates total)
    /// <summary>
    /// Miktarı günceller ve toplam fiyatı yeniden hesaplar
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));

        Quantity = newQuantity;
        // ✅ BOLUM 1.6: Invariant validation - TotalPrice = UnitPrice * Quantity
        TotalPrice = UnitPrice * Quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update unit price (recalculates total)
    /// <summary>
    /// Birim fiyatı günceller ve toplam fiyatı yeniden hesaplar
    /// </summary>
    public void UpdateUnitPrice(Money newUnitPrice)
    {
        Guard.AgainstNegative(newUnitPrice.Amount, nameof(newUnitPrice));

        UnitPrice = newUnitPrice.Amount;
        // ✅ BOLUM 1.6: Invariant validation - TotalPrice = UnitPrice * Quantity
        TotalPrice = UnitPrice * Quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}

