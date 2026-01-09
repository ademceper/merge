using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// BlogCategory Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class BlogCategory : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public BlogCategory? ParentCategory { get; private set; }
    public ICollection<BlogCategory> SubCategories { get; private set; } = new List<BlogCategory>();
    public ICollection<BlogPost> Posts { get; private set; } = new List<BlogPost>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private BlogCategory() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static BlogCategory Create(
        string name,
        string? description = null,
        Guid? parentCategoryId = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isActive = true,
        string? slug = null) // Optional slug for uniqueness handling
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));

        if (parentCategoryId.HasValue && parentCategoryId.Value == Guid.Empty)
        {
            throw new DomainException("Geçersiz parent category ID.");
        }

        var finalSlug = slug ?? GenerateSlug(name);

        var category = new BlogCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = finalSlug,
            Description = description,
            ParentCategoryId = parentCategoryId,
            ImageUrl = imageUrl,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - BlogCategoryCreatedEvent yayınla (ÖNERİLİR)
        category.AddDomainEvent(new BlogCategoryCreatedEvent(category.Id, name, finalSlug));

        return category;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update name
    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrEmpty(newName, nameof(newName));
        Name = newName;
        Slug = GenerateSlug(newName);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, newName, Slug));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update description
    public void UpdateDescription(string? newDescription)
    {
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update parent category
    public void UpdateParentCategory(Guid? parentCategoryId)
    {
        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
        {
            throw new DomainException("Kategori kendisinin alt kategorisi olamaz.");
        }
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update image URL
    public void UpdateImageUrl(string? newImageUrl)
    {
        ImageUrl = newImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update display order
    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));
        DisplayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate
    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate
    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryDeletedEvent(Id, Name));
    }

    // ✅ BOLUM 1.3: Slug generation helper
    public static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "");

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}

