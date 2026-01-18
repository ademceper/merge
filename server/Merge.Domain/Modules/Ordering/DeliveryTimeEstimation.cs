using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// DeliveryTimeEstimation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU - Optional, navigation property için gerekli değil)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DeliveryTimeEstimation : BaseEntity, IAggregateRoot
{
    public Guid? ProductId { get; private set; } // Product-specific estimation
    public Guid? CategoryId { get; private set; } // Category-based estimation
    public Guid? WarehouseId { get; private set; } // Warehouse-specific estimation
    public Guid? ShippingProviderId { get; private set; } // Provider-specific estimation
    public string? City { get; private set; } // City-specific estimation
    public string? Country { get; private set; } // Country-specific estimation
    
    private int _minDays = 0;
    public int MinDays 
    { 
        get => _minDays; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(MinDays));
            _minDays = value;
        } 
    }
    
    private int _maxDays = 0;
    public int MaxDays 
    { 
        get => _maxDays; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(MaxDays));
            _maxDays = value;
        } 
    }
    
    private int _averageDays = 0;
    public int AverageDays 
    { 
        get => _averageDays; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(AverageDays));
            _averageDays = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;
    public string? Conditions { get; private set; } // JSON for conditions (e.g., stock availability, order time)
    
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    // Navigation properties
    public Product? Product { get; private set; }
    public Category? Category { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    private DeliveryTimeEstimation() { }

    public static DeliveryTimeEstimation Create(
        int minDays,
        int maxDays,
        int averageDays,
        Guid? productId = null,
        Guid? categoryId = null,
        Guid? warehouseId = null,
        Guid? shippingProviderId = null,
        string? city = null,
        string? country = null,
        string? conditions = null,
        bool isActive = true)
    {
        Guard.AgainstNegative(minDays, nameof(minDays));
        Guard.AgainstNegative(maxDays, nameof(maxDays));
        Guard.AgainstNegative(averageDays, nameof(averageDays));

        if (minDays > maxDays)
            throw new DomainException("Minimum gün sayısı maksimum gün sayısından büyük olamaz.");

        if (averageDays < minDays || averageDays > maxDays)
            throw new DomainException("Ortalama gün sayısı minimum ve maksimum gün sayıları arasında olmalıdır.");

        var estimation = new DeliveryTimeEstimation
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            CategoryId = categoryId,
            WarehouseId = warehouseId,
            ShippingProviderId = shippingProviderId,
            City = city,
            Country = country,
            _minDays = minDays,
            _maxDays = maxDays,
            _averageDays = averageDays,
            Conditions = conditions,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

        estimation.AddDomainEvent(new DeliveryTimeEstimationCreatedEvent(
            estimation.Id,
            estimation.ProductId,
            estimation.CategoryId,
            estimation.WarehouseId,
            estimation.MinDays,
            estimation.MaxDays,
            estimation.AverageDays));

        return estimation;
    }

    public void UpdateDays(int minDays, int maxDays, int averageDays)
    {
        Guard.AgainstNegative(minDays, nameof(minDays));
        Guard.AgainstNegative(maxDays, nameof(maxDays));
        Guard.AgainstNegative(averageDays, nameof(averageDays));

        if (minDays > maxDays)
            throw new DomainException("Minimum gün sayısı maksimum gün sayısından büyük olamaz.");

        if (averageDays < minDays || averageDays > maxDays)
            throw new DomainException("Ortalama gün sayısı minimum ve maksimum gün sayıları arasında olmalıdır.");

        _minDays = minDays;
        _maxDays = maxDays;
        _averageDays = averageDays;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DeliveryTimeEstimationUpdatedEvent(Id, _minDays, _maxDays, _averageDays));
    }

    public void UpdateConditions(string? conditions)
    {
        Conditions = conditions;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DeliveryTimeEstimationConditionsUpdatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DeliveryTimeEstimationActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DeliveryTimeEstimationDeactivatedEvent(Id));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new DeliveryTimeEstimationDeletedEvent(Id));
    }
}

