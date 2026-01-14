using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Inventory;

/// <summary>
/// PickPackItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PickPackItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid PickPackId { get; private set; }
    public Guid OrderItemId { get; private set; }
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
    
    public bool IsPicked { get; private set; } = false;
    public bool IsPacked { get; private set; } = false;
    public DateTime? PickedAt { get; private set; }
    public DateTime? PackedAt { get; private set; }
    public string? Location { get; private set; } // Warehouse location (Aisle-Shelf-Bin)
    
    // Navigation properties
    public PickPack PickPack { get; private set; } = null!;
    public OrderItem OrderItem { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PickPackItem() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PickPackItem Create(
        Guid pickPackId,
        Guid orderItemId,
        Guid productId,
        int quantity,
        string? location = null)
    {
        Guard.AgainstDefault(pickPackId, nameof(pickPackId));
        Guard.AgainstDefault(orderItemId, nameof(orderItemId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        return new PickPackItem
        {
            Id = Guid.NewGuid(),
            PickPackId = pickPackId,
            OrderItemId = orderItemId,
            ProductId = productId,
            _quantity = quantity,
            Location = location,
            IsPicked = false,
            IsPacked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as picked
    public void MarkAsPicked()
    {
        if (IsPicked) return;

        IsPicked = true;
        PickedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as packed
    public void MarkAsPacked()
    {
        if (!IsPicked)
            throw new DomainException("Önce toplanmalıdır.");

        if (IsPacked) return;

        IsPacked = true;
        PackedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update location
    public void UpdateLocation(string? location)
    {
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

