using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Content;

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
    // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı (ZORUNLU)
    public Slug Slug { get; private set; } = null!;
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
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<BlogCategory> _subCategories = new();
    public IReadOnlyCollection<BlogCategory> SubCategories => _subCategories.AsReadOnly();
    
    private readonly List<BlogPost> _posts = new();
    public IReadOnlyCollection<BlogPost> Posts => _posts.AsReadOnly();

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

        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        var finalSlug = slug != null ? Slug.FromString(slug) : Slug.FromString(name);

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
        category.AddDomainEvent(new BlogCategoryCreatedEvent(category.Id, name, finalSlug.Value));

        return category;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update name
    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrEmpty(newName, nameof(newName));
        Name = newName;
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        Slug = Slug.FromString(newName);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, newName, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update description
    public void UpdateDescription(string? newDescription)
    {
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
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
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update image URL
    public void UpdateImageUrl(string? newImageUrl)
    {
        ImageUrl = newImageUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update display order
    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));
        DisplayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryActivatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryActivatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryDeactivatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryDeactivatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCategoryDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCategoryDeletedEvent(Id, Name));
    }

}

