using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Inventory;

/// <summary>
/// PickPack Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PickPack : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string PackNumber { get; private set; } = string.Empty; // Auto-generated: PK-XXXXXX
    public PickPackStatus Status { get; private set; } = PickPackStatus.Pending;
    public Guid? PickedByUserId { get; private set; } // Staff who picked the items
    public Guid? PackedByUserId { get; private set; } // Staff who packed the items
    public DateTime? PickedAt { get; private set; }
    public DateTime? PackedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public string? Notes { get; private set; }
    
    private decimal _weight = 0;
    public decimal Weight 
    { 
        get => _weight; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(Weight));
            _weight = value;
        } 
    }
    
    public string? Dimensions { get; private set; } // Length x Width x Height in cm
    
    private int _packageCount = 1;
    public int PackageCount 
    { 
        get => _packageCount; 
        private set 
        { 
            Guard.AgainstNegativeOrZero(value, nameof(PackageCount));
            _packageCount = value;
        } 
    }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;
    public User? PickedBy { get; private set; }
    public User? PackedBy { get; private set; }
    public ICollection<PickPackItem> Items { get; private set; } = [];

    private PickPack() { }

    public static PickPack Create(
        Guid orderId,
        Guid warehouseId,
        string packNumber,
        string? notes = null)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstDefault(warehouseId, nameof(warehouseId));
        Guard.AgainstNullOrEmpty(packNumber, nameof(packNumber));

        var pickPack = new PickPack
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            WarehouseId = warehouseId,
            PackNumber = packNumber,
            Status = PickPackStatus.Pending,
            Notes = notes,
            _packageCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        pickPack.AddDomainEvent(new PickPackCreatedEvent(pickPack.Id, pickPack.OrderId, pickPack.WarehouseId, pickPack.PackNumber));

        return pickPack;
    }

    public void StartPicking(Guid pickedByUserId)
    {
        Guard.AgainstDefault(pickedByUserId, nameof(pickedByUserId));

        if (Status != PickPackStatus.Pending)
            throw new DomainException("Sadece bekleyen pick-pack'ler toplamaya başlanabilir.");

        var oldStatus = Status;
        Status = PickPackStatus.Picking;
        PickedByUserId = pickedByUserId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PickPackStatusChangedEvent(Id, OrderId, oldStatus, Status));
    }

    public void CompletePicking()
    {
        if (Status != PickPackStatus.Picking)
            throw new DomainException("Sadece toplama aşamasındaki pick-pack'ler tamamlanabilir.");

        var oldStatus = Status;
        Status = PickPackStatus.Picked;
        PickedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PickPackStatusChangedEvent(Id, OrderId, oldStatus, Status));
    }

    public void StartPacking(Guid packedByUserId)
    {
        Guard.AgainstDefault(packedByUserId, nameof(packedByUserId));

        if (Status != PickPackStatus.Picked)
            throw new DomainException("Sadece toplanmış pick-pack'ler paketlemeye başlanabilir.");

        var oldStatus = Status;
        Status = PickPackStatus.Packing;
        PackedByUserId = packedByUserId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PickPackStatusChangedEvent(Id, OrderId, oldStatus, Status));
    }

    public void CompletePacking(decimal weight, string? dimensions = null, int packageCount = 1)
    {
        if (Status != PickPackStatus.Packing)
            throw new DomainException("Sadece paketleme aşamasındaki pick-pack'ler tamamlanabilir.");

        Guard.AgainstNegativeOrZero(weight, nameof(weight));
        Guard.AgainstNegativeOrZero(packageCount, nameof(packageCount));

        var oldStatus = Status;
        Status = PickPackStatus.Packed;
        PackedAt = DateTime.UtcNow;
        _weight = weight;
        Dimensions = dimensions;
        _packageCount = packageCount;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PickPackStatusChangedEvent(Id, OrderId, oldStatus, Status));
    }

    public void Ship()
    {
        if (Status != PickPackStatus.Packed)
            throw new DomainException("Sadece paketlenmiş pick-pack'ler kargoya verilebilir.");

        var oldStatus = Status;
        Status = PickPackStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PickPackStatusChangedEvent(Id, OrderId, oldStatus, Status));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == PickPackStatus.Shipped)
            throw new DomainException("Kargoya verilmiş pick-pack'ler iptal edilemez.");

        var oldStatus = Status;
        Status = PickPackStatus.Cancelled;
        Notes = reason != null ? $"{Notes}\nİptal nedeni: {reason}".Trim() : Notes;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PickPackStatusChangedEvent(Id, OrderId, oldStatus, Status));
    }

    public void UpdateDetails(
        string? notes,
        decimal? weight,
        string? dimensions,
        int? packageCount)
    {
        if (weight.HasValue)
        {
            Guard.AgainstNegative(weight.Value, nameof(weight));
            _weight = weight.Value;
        }
        if (packageCount.HasValue)
        {
            Guard.AgainstNegativeOrZero(packageCount.Value, nameof(packageCount));
            _packageCount = packageCount.Value;
        }

        Notes = notes;
        Dimensions = dimensions;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PickPackDetailsUpdatedEvent(Id, OrderId));
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PickPackDetailsUpdatedEvent(Id, OrderId));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        if (Status == PickPackStatus.Shipped)
            throw new DomainException("Kargoya verilmiş pick-pack'ler silinemez.");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

