using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// Warehouse Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Warehouse : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty; // Unique warehouse code (WH001, WH002, etc.)
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string ContactPerson { get; private set; } = string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
    public string ContactEmail { get; private set; } = string.Empty;
    
    private int _capacity = 0;
    public int Capacity 
    { 
        get => _capacity; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(Capacity));
            _capacity = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }

    // Navigation properties
    public ICollection<Inventory> Inventories { get; private set; } = new List<Inventory>();
    public ICollection<StockMovement> StockMovements { get; private set; } = new List<StockMovement>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Warehouse() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Warehouse Create(
        string name,
        string code,
        string address,
        string city,
        string country,
        string postalCode,
        string contactPerson,
        string contactPhone,
        string contactEmail,
        int capacity,
        string? description = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(code, nameof(code));
        Guard.AgainstNullOrEmpty(address, nameof(address));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstNullOrEmpty(country, nameof(country));
        Guard.AgainstNullOrEmpty(contactPerson, nameof(contactPerson));
        Guard.AgainstNullOrEmpty(contactPhone, nameof(contactPhone));
        Guard.AgainstNullOrEmpty(contactEmail, nameof(contactEmail));
        Guard.AgainstNegativeOrZero(capacity, nameof(capacity));

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Address = address,
            City = city,
            Country = country,
            PostalCode = postalCode,
            ContactPerson = contactPerson,
            ContactPhone = contactPhone,
            ContactEmail = contactEmail,
            _capacity = capacity,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - WarehouseCreatedEvent
        warehouse.AddDomainEvent(new WarehouseCreatedEvent(warehouse.Id, warehouse.Name, warehouse.Code));

        return warehouse;
    }

    // ✅ BOLUM 1.1: Domain Method - Update warehouse details
    public void UpdateDetails(
        string name,
        string address,
        string city,
        string country,
        string postalCode,
        string contactPerson,
        string contactPhone,
        string contactEmail,
        int capacity,
        string? description = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(address, nameof(address));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstNullOrEmpty(country, nameof(country));
        Guard.AgainstNullOrEmpty(contactPerson, nameof(contactPerson));
        Guard.AgainstNullOrEmpty(contactPhone, nameof(contactPhone));
        Guard.AgainstNullOrEmpty(contactEmail, nameof(contactEmail));
        Guard.AgainstNegativeOrZero(capacity, nameof(capacity));

        Name = name;
        Address = address;
        City = city;
        Country = country;
        PostalCode = postalCode;
        ContactPerson = contactPerson;
        ContactPhone = contactPhone;
        ContactEmail = contactEmail;
        _capacity = capacity;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - WarehouseUpdatedEvent
        AddDomainEvent(new WarehouseUpdatedEvent(Id, Name, Code));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate warehouse
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - WarehouseActivatedEvent
        AddDomainEvent(new WarehouseActivatedEvent(Id));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate warehouse
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - WarehouseDeactivatedEvent
        AddDomainEvent(new WarehouseDeactivatedEvent(Id));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
