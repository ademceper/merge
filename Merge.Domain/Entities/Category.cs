using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

/// <summary>
/// Category Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class Category : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Slug
    private string _slug = string.Empty;
    public string Slug 
    { 
        get => _slug; 
        private set => _slug = value; 
    }
    
    public string ImageUrl { get; private set; } = string.Empty;
    public Guid? ParentCategoryId { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Category? ParentCategory { get; private set; }
    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Category() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Category Create(
        string name,
        string description,
        string slug,
        string? imageUrl = null,
        Guid? parentCategoryId = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstNullOrEmpty(slug, nameof(slug));

        // ✅ BOLUM 1.3: Slug validation
        if (!IsValidSlug(slug))
        {
            throw new DomainException("Geçersiz slug formatı. Slug sadece küçük harf, rakam ve tire içerebilir.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            _slug = slug.ToLowerInvariant(),
            ImageUrl = imageUrl ?? string.Empty,
            ParentCategoryId = parentCategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - CategoryCreatedEvent yayınla (ÖNERİLİR)
        category.AddDomainEvent(new CategoryCreatedEvent(category.Id, name, slug, parentCategoryId));

        return category;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update name
    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrEmpty(newName, nameof(newName));
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CategoryUpdatedEvent(Id, newName, _slug));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update description
    public void UpdateDescription(string newDescription)
    {
        Guard.AgainstNull(newDescription, nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update slug
    public void UpdateSlug(string newSlug)
    {
        Guard.AgainstNullOrEmpty(newSlug, nameof(newSlug));
        if (!IsValidSlug(newSlug))
        {
            throw new DomainException("Geçersiz slug formatı. Slug sadece küçük harf, rakam ve tire içerebilir.");
        }
        _slug = newSlug.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CategoryUpdatedEvent(Id, Name, _slug));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update image URL
    public void UpdateImageUrl(string? newImageUrl)
    {
        ImageUrl = newImageUrl ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set parent category
    public void SetParentCategory(Guid? parentCategoryId)
    {
        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
        {
            throw new DomainException("Kategori kendisinin alt kategorisi olamaz.");
        }
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CategoryDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CategoryDeletedEvent(Id, Name));
    }

    // ✅ BOLUM 1.3: Slug validation helper
    private static bool IsValidSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        var normalized = slug.Trim().ToLowerInvariant();
        return System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }
}

