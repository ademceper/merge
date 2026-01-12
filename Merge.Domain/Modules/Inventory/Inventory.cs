using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Inventory;

/// <summary>
/// Inventory Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// </summary>
public class Inventory : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    
    // ✅ BOLUM 1.6: Invariant validation - Quantity >= 0
    private int _quantity = 0;
    public int Quantity 
    { 
        get => _quantity; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(Quantity));
            _quantity = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - ReservedQuantity >= 0
    private int _reservedQuantity = 0;
    public int ReservedQuantity 
    { 
        get => _reservedQuantity; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(ReservedQuantity));
            if (value > _quantity)
            {
                throw new DomainException("Rezerve edilen miktar toplam miktardan fazla olamaz.");
            }
            _reservedQuantity = value;
        }
    }
    
    public int AvailableQuantity => Quantity - ReservedQuantity; // Calculated property
    
    // ✅ BOLUM 1.6: Invariant validation - MinimumStockLevel >= 0
    private int _minimumStockLevel = 0;
    public int MinimumStockLevel 
    { 
        get => _minimumStockLevel; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(MinimumStockLevel));
            _minimumStockLevel = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - MaximumStockLevel >= 0
    private int _maximumStockLevel = 0;
    public int MaximumStockLevel 
    { 
        get => _maximumStockLevel; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(MaximumStockLevel));
            if (value > 0 && _minimumStockLevel > value)
            {
                throw new DomainException("Minimum stok seviyesi maksimum stok seviyesinden büyük olamaz.");
            }
            _maximumStockLevel = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - UnitCost >= 0
    private decimal _unitCost = 0;
    public decimal UnitCost 
    { 
        get => _unitCost; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(UnitCost));
            _unitCost = value;
        }
    }
    
    public string? Location { get; private set; }
    public DateTime? LastRestockedAt { get; private set; }
    public DateTime? LastCountedAt { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;
    public ICollection<StockMovement> StockMovements { get; private set; } = new List<StockMovement>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Inventory() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Inventory Create(
        Guid productId,
        Guid warehouseId,
        int quantity,
        int minimumStockLevel = 0,
        int maximumStockLevel = 0,
        decimal unitCost = 0,
        string? location = null)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(warehouseId, nameof(warehouseId));
        Guard.AgainstNegative(quantity, nameof(quantity));
        Guard.AgainstNegative(minimumStockLevel, nameof(minimumStockLevel));
        Guard.AgainstNegative(maximumStockLevel, nameof(maximumStockLevel));
        Guard.AgainstNegative(unitCost, nameof(unitCost));

        if (maximumStockLevel > 0 && minimumStockLevel > maximumStockLevel)
        {
            throw new DomainException("Minimum stok seviyesi maksimum stok seviyesinden büyük olamaz.");
        }

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            _quantity = quantity,
            _minimumStockLevel = minimumStockLevel,
            _maximumStockLevel = maximumStockLevel,
            _unitCost = unitCost,
            Location = location,
            LastRestockedAt = quantity > 0 ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - InventoryCreatedEvent yayınla (ÖNERİLİR)
        inventory.AddDomainEvent(new InventoryCreatedEvent(inventory.Id, productId, warehouseId, quantity));

        return inventory;
    }

    // ✅ BOLUM 1.1: Domain Logic - Adjust quantity
    public void AdjustQuantity(int quantityChange)
    {
        var newQuantity = _quantity + quantityChange;
        if (newQuantity < 0)
        {
            throw new DomainException("Stok miktarı sıfırın altına düşürülemez.");
        }

        if (newQuantity < _reservedQuantity)
        {
            throw new DomainException("Stok miktarı rezerve edilen miktardan az olamaz.");
        }

        _quantity = newQuantity;
        if (quantityChange > 0)
        {
            LastRestockedAt = DateTime.UtcNow;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Reserve stock
    public void Reserve(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        
        if (_reservedQuantity + quantity > _quantity)
        {
            throw new DomainException("Rezerve edilecek miktar mevcut stoktan fazla olamaz.");
        }

        _reservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Release reserved stock
    public void ReleaseReserved(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        
        if (quantity > _reservedQuantity)
        {
            throw new DomainException("Serbest bırakılacak miktar rezerve edilmiş miktardan fazla olamaz.");
        }

        _reservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Release and reduce stock (for sales)
    public void ReleaseAndReduce(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        
        if (quantity > _reservedQuantity)
        {
            throw new DomainException("Serbest bırakılacak miktar rezerve edilmiş miktardan fazla olamaz.");
        }

        _reservedQuantity -= quantity;
        _quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update stock levels
    public void UpdateStockLevels(int minimumStockLevel, int maximumStockLevel)
    {
        Guard.AgainstNegative(minimumStockLevel, nameof(minimumStockLevel));
        Guard.AgainstNegative(maximumStockLevel, nameof(maximumStockLevel));

        if (maximumStockLevel > 0 && minimumStockLevel > maximumStockLevel)
        {
            throw new DomainException("Minimum stok seviyesi maksimum stok seviyesinden büyük olamaz.");
        }

        _minimumStockLevel = minimumStockLevel;
        _maximumStockLevel = maximumStockLevel;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InventoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new InventoryUpdatedEvent(Id, ProductId, WarehouseId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update unit cost
    public void UpdateUnitCost(decimal unitCost)
    {
        Guard.AgainstNegative(unitCost, nameof(unitCost));
        _unitCost = unitCost;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InventoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new InventoryUpdatedEvent(Id, ProductId, WarehouseId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update location
    public void UpdateLocation(string? location)
    {
        Location = location;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InventoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new InventoryUpdatedEvent(Id, ProductId, WarehouseId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update last counted date
    public void UpdateLastCountedDate()
    {
        LastCountedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if low stock
    public bool IsLowStock()
    {
        return _maximumStockLevel > 0 && _quantity <= _minimumStockLevel;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    // Note: Inventory için soft delete kullanılabilir, ancak stok 0 ise hard delete de mantıklı olabilir
    public void MarkAsDeleted()
    {
        if (_quantity > 0)
        {
            throw new DomainException("Stoklu envanter silinemez. Önce stoku sıfırlayın.");
        }
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
