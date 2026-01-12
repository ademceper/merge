using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// SizeGuide Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class SizeGuide : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Name));
            if (value.Length < 2)
            {
                throw new DomainException("Beden kılavuzu adı en az 2 karakter olmalıdır");
            }
            if (value.Length > 200)
            {
                throw new DomainException("Beden kılavuzu adı en fazla 200 karakter olabilir");
            }
            _name = value;
        } 
    }
    
    public string Description { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string? Brand { get; private set; }
    public SizeGuideType Type { get; private set; } = SizeGuideType.Standard;
    
    private string _measurementUnit = "cm";
    public string MeasurementUnit 
    { 
        get => _measurementUnit; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(MeasurementUnit));
            if (value.Length > 20)
            {
                throw new DomainException("Ölçü birimi en fazla 20 karakter olabilir");
            }
            _measurementUnit = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;
    
    // Navigation properties
    private readonly List<SizeGuideEntry> _entries = new();
    public IReadOnlyCollection<SizeGuideEntry> Entries => _entries.AsReadOnly();
    public ICollection<ProductSizeGuide> ProductSizeGuides { get; private set; } = new List<ProductSizeGuide>();
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SizeGuide() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static SizeGuide Create(
        string name,
        string description,
        Guid categoryId,
        SizeGuideType type = SizeGuideType.Standard,
        string? brand = null,
        string measurementUnit = "cm")
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstDefault(categoryId, nameof(categoryId));
        Guard.AgainstNullOrEmpty(measurementUnit, nameof(measurementUnit));
        
        if (name.Length < 2)
        {
            throw new DomainException("Beden kılavuzu adı en az 2 karakter olmalıdır");
        }
        if (name.Length > 200)
        {
            throw new DomainException("Beden kılavuzu adı en fazla 200 karakter olabilir");
        }
        
        var sizeGuide = new SizeGuide
        {
            Id = Guid.NewGuid(),
            _name = name,
            Description = description ?? string.Empty,
            CategoryId = categoryId,
            Brand = brand,
            Type = type,
            _measurementUnit = measurementUnit,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.5: Domain Events
        sizeGuide.AddDomainEvent(new SizeGuideCreatedEvent(sizeGuide.Id, name, categoryId));
        
        return sizeGuide;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Update
    public void Update(
        string name,
        string? description = null,
        Guid? categoryId = null,
        SizeGuideType? type = null,
        string? brand = null,
        string? measurementUnit = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        
        Name = name;
        if (description != null) Description = description;
        if (categoryId.HasValue) CategoryId = categoryId.Value;
        if (type.HasValue) Type = type.Value;
        if (brand != null) Brand = brand;
        if (measurementUnit != null) MeasurementUnit = measurementUnit;
        
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Add entry
    public void AddEntry(SizeGuideEntry entry)
    {
        Guard.AgainstNull(entry, nameof(entry));
        
        if (_entries.Any(e => e.SizeLabel == entry.SizeLabel))
        {
            throw new DomainException("Bu beden etiketi zaten mevcut");
        }
        
        // EF Core will set SizeGuideId automatically through navigation property
        _entries.Add(entry);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SizeGuideUpdatedEvent yayınla (ÖNERİLİR)
        // Entry ekleme önemli bir business event'tir
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Remove entry
    public void RemoveEntry(Guid entryId)
    {
        Guard.AgainstDefault(entryId, nameof(entryId));
        
        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry == null)
        {
            throw new DomainException("Beden girişi bulunamadı");
        }
        
        _entries.Remove(entry);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SizeGuideUpdatedEvent yayınla (ÖNERİLİR)
        // Entry çıkarma önemli bir business event'tir
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Activate
    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SizeGuideUpdatedEvent yayınla (ÖNERİLİR)
        // Aktif/pasif durumu önemli bir business event'tir
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Deactivate
    public void Deactivate()
    {
        if (!IsActive) return;
        
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SizeGuideUpdatedEvent yayınla (ÖNERİLİR)
        // Aktif/pasif durumu önemli bir business event'tir
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new SizeGuideDeletedEvent(Id, Name));
    }
}

