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
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    private readonly List<SizeGuideEntry> _entries = new();
    public IReadOnlyCollection<SizeGuideEntry> Entries => _entries.AsReadOnly();
    
    private readonly List<ProductSizeGuide> _productSizeGuides = new();
    public IReadOnlyCollection<ProductSizeGuide> ProductSizeGuides => _productSizeGuides.AsReadOnly();
    
    private SizeGuide() { }
    
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
        
        sizeGuide.ValidateInvariants();
        
        sizeGuide.AddDomainEvent(new SizeGuideCreatedEvent(sizeGuide.Id, name, categoryId));
        
        return sizeGuide;
    }
    
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
        if (description is not null) Description = description;
        if (categoryId.HasValue) CategoryId = categoryId.Value;
        if (type.HasValue) Type = type.Value;
        if (brand is not null) Brand = brand;
        if (measurementUnit is not null) MeasurementUnit = measurementUnit;
        
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
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
        
        ValidateInvariants();
        
        // Entry ekleme önemli bir business event'tir
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    public void RemoveEntry(Guid entryId)
    {
        Guard.AgainstDefault(entryId, nameof(entryId));
        
        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry is null)
        {
            throw new DomainException("Beden girişi bulunamadı");
        }
        
        _entries.Remove(entry);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        // Entry çıkarma önemli bir business event'tir
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        // Aktif/pasif durumu önemli bir business event'tir
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    public void Deactivate()
    {
        if (!IsActive) return;
        
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        // Aktif/pasif durumu önemli bir business event'tir
        AddDomainEvent(new SizeGuideUpdatedEvent(Id, Name));
    }
    
    public void AddProductSizeGuide(ProductSizeGuide productSizeGuide)
    {
        Guard.AgainstNull(productSizeGuide, nameof(productSizeGuide));
        if (productSizeGuide.SizeGuideId != Id)
        {
            throw new DomainException("Product size guide bu size guide'a ait değil");
        }
        if (_productSizeGuides.Any(psg => psg.Id == productSizeGuide.Id))
        {
            throw new DomainException("Bu product size guide zaten eklenmiş");
        }
        _productSizeGuides.Add(productSizeGuide);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void RemoveProductSizeGuide(Guid productSizeGuideId)
    {
        Guard.AgainstDefault(productSizeGuideId, nameof(productSizeGuideId));
        var productSizeGuide = _productSizeGuides.FirstOrDefault(psg => psg.Id == productSizeGuideId);
        if (productSizeGuide is null)
        {
            throw new DomainException("Product size guide bulunamadı");
        }
        _productSizeGuides.Remove(productSizeGuide);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SizeGuideDeletedEvent(Id, Name));
    }

    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new DomainException("Beden kılavuzu adı boş olamaz");

        if (_name.Length < 2 || _name.Length > 200)
            throw new DomainException("Beden kılavuzu adı 2-200 karakter arasında olmalıdır");

        if (Guid.Empty == CategoryId)
            throw new DomainException("Kategori ID boş olamaz");

        if (string.IsNullOrWhiteSpace(_measurementUnit))
            throw new DomainException("Ölçü birimi boş olamaz");

        if (_measurementUnit.Length > 20)
            throw new DomainException("Ölçü birimi en fazla 20 karakter olabilir");
    }
}

