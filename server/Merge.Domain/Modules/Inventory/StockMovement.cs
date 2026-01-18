using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Inventory;

/// <summary>
/// StockMovement Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class StockMovement : BaseEntity, IAggregateRoot
{
    public Guid InventoryId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    
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
    
    public int QuantityBefore { get; private set; } // Quantity before this movement
    public int QuantityAfter { get; private set; } // Quantity after this movement
    public string? ReferenceNumber { get; private set; } // PO number, Order number, etc.
    public Guid? ReferenceId { get; private set; } // Reference to Order, Purchase Order, etc.
    public string? Notes { get; private set; }
    public Guid? PerformedBy { get; private set; } // User who performed the movement
    public Guid? FromWarehouseId { get; private set; } // For transfers
    public Guid? ToWarehouseId { get; private set; } // For transfers

    // Navigation properties
    public Inventory Inventory { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;
    public User? User { get; private set; }
    public Warehouse? FromWarehouse { get; private set; }
    public Warehouse? ToWarehouse { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(
        Guid inventoryId,
        Guid productId,
        Guid warehouseId,
        StockMovementType movementType,
        int quantity,
        int quantityBefore,
        int quantityAfter,
        Guid? performedBy = null,
        string? referenceNumber = null,
        Guid? referenceId = null,
        string? notes = null,
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null)
    {
        Guard.AgainstDefault(inventoryId, nameof(inventoryId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(warehouseId, nameof(warehouseId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNegative(quantityBefore, nameof(quantityBefore));
        Guard.AgainstNegative(quantityAfter, nameof(quantityAfter));

        if (quantityAfter < 0)
            throw new DomainException("Stok miktarı negatif olamaz.");

        var stockMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            InventoryId = inventoryId,
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = movementType,
            _quantity = quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            PerformedBy = performedBy,
            ReferenceNumber = referenceNumber,
            ReferenceId = referenceId,
            Notes = notes,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = toWarehouseId,
            CreatedAt = DateTime.UtcNow
        };

        stockMovement.AddDomainEvent(new StockMovementCreatedEvent(
            stockMovement.Id,
            stockMovement.ProductId,
            stockMovement.WarehouseId,
            stockMovement.MovementType,
            stockMovement.Quantity,
            stockMovement.QuantityBefore,
            stockMovement.QuantityAfter));

        return stockMovement;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

