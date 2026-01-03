using Merge.Domain.Common;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Entities;

/// <summary>
/// PurchaseOrderItem Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate içinde entity (PurchaseOrder Aggregate Root)
/// </summary>
public class PurchaseOrderItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid PurchaseOrderId { get; private set; }
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
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - EF Core compatibility için decimal backing fields
    private decimal _unitPrice;
    private decimal _totalPrice;
    
    // Database columns (EF Core mapping)
    public decimal UnitPrice 
    { 
        get => _unitPrice; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(UnitPrice));
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
    
    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money UnitPriceMoney => new Money(_unitPrice);
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money TotalPriceMoney => new Money(_totalPrice);
    
    public string? Notes { get; private set; }
    
    // Navigation properties
    public PurchaseOrder PurchaseOrder { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PurchaseOrderItem() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    // PurchaseOrderItem Aggregate içinde entity olduğu için PurchaseOrder üzerinden oluşturulur
    // Ancak factory method ile invariant'ları koruyoruz
    public static PurchaseOrderItem Create(
        Guid purchaseOrderId,
        Guid productId,
        Product product,
        int quantity,
        decimal unitPrice,
        string? notes = null)
    {
        Guard.AgainstDefault(purchaseOrderId, nameof(purchaseOrderId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNegativeOrZero(unitPrice, nameof(unitPrice));

        var totalPrice = unitPrice * quantity;

        // ✅ BOLUM 1.6: Invariant validation - TotalPrice = UnitPrice * Quantity
        if (totalPrice != unitPrice * quantity)
            throw new DomainException("Toplam fiyat hesaplama hatası");

        var item = new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            Product = product,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = totalPrice,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        return item;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update quantity (recalculates total)
    public void UpdateQuantity(int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));

        Quantity = newQuantity;
        // ✅ BOLUM 1.6: Invariant validation - TotalPrice = UnitPrice * Quantity
        TotalPrice = UnitPrice * Quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update unit price (recalculates total)
    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        Guard.AgainstNegativeOrZero(newUnitPrice, nameof(newUnitPrice));

        UnitPrice = newUnitPrice;
        // ✅ BOLUM 1.6: Invariant validation - TotalPrice = UnitPrice * Quantity
        TotalPrice = UnitPrice * Quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update notes
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

