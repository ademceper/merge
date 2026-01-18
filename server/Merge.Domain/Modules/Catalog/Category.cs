using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// Category Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class Category : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    private string _slug = string.Empty;
    public string Slug 
    { 
        get => _slug; 
        private set => _slug = value; 
    }
    
    public string ImageUrl { get; private set; } = string.Empty;
    public Guid? ParentCategoryId { get; private set; }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    [NotMapped]
    public Slug SlugValueObject => new Slug(_slug);
    
    // Navigation properties
    public Category? ParentCategory { get; private set; }
    
    private readonly List<Category> _subCategories = new();
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();
    
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() { }

    public static Category Create(
        string name,
        string description,
        Slug slug,
        string? imageUrl = null,
        Guid? parentCategoryId = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstNull(slug, nameof(slug));

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            _slug = slug.Value,
            ImageUrl = imageUrl ?? string.Empty,
            ParentCategoryId = parentCategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        category.ValidateInvariants();

        category.AddDomainEvent(new CategoryCreatedEvent(category.Id, name, slug.Value, parentCategoryId));

        return category;
    }

    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrEmpty(newName, nameof(newName));
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new CategoryUpdatedEvent(Id, newName, _slug));
    }

    public void UpdateDescription(string newDescription)
    {
        Guard.AgainstNull(newDescription, nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new CategoryUpdatedEvent(Id, Name, _slug));
    }

    public void UpdateSlug(Slug newSlug)
    {
        Guard.AgainstNull(newSlug, nameof(newSlug));
        _slug = newSlug.Value;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new CategoryUpdatedEvent(Id, Name, _slug));
    }

    public void UpdateImageUrl(string? newImageUrl)
    {
        ImageUrl = newImageUrl ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void SetParentCategory(Guid? parentCategoryId)
    {
        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
        {
            throw new DomainException("Kategori kendisinin alt kategorisi olamaz.");
        }
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void AddSubCategory(Category subCategory)
    {
        Guard.AgainstNull(subCategory, nameof(subCategory));
        if (subCategory.ParentCategoryId != Id)
        {
            throw new DomainException("Alt kategori bu kategoriye ait değil");
        }
        if (_subCategories.Any(c => c.Id == subCategory.Id))
        {
            throw new DomainException("Bu alt kategori zaten eklenmiş");
        }
        _subCategories.Add(subCategory);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void RemoveSubCategory(Guid subCategoryId)
    {
        Guard.AgainstDefault(subCategoryId, nameof(subCategoryId));
        var subCategory = _subCategories.FirstOrDefault(c => c.Id == subCategoryId);
        if (subCategory == null)
        {
            throw new DomainException("Alt kategori bulunamadı");
        }
        _subCategories.Remove(subCategory);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new CategoryDeletedEvent(Id, Name));
    }

    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Kategori adı boş olamaz");

        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException("Kategori açıklaması boş olamaz");

        if (string.IsNullOrWhiteSpace(_slug))
            throw new DomainException("Kategori slug'ı boş olamaz");

        // Slug Value Object validation zaten constructor'da yapılıyor
        // Burada sadece boş olup olmadığını kontrol ediyoruz
        try
        {
            var slug = new Slug(_slug); // Validation için
        }
        catch (ArgumentException)
        {
            throw new DomainException("Geçersiz slug formatı");
        }

        if (ParentCategoryId.HasValue && ParentCategoryId.Value == Id)
            throw new DomainException("Kategori kendisinin alt kategorisi olamaz");
    }
}

