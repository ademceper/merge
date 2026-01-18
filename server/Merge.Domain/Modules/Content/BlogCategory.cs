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
    public string Name { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public BlogCategory? ParentCategory { get; private set; }
    
    private readonly List<BlogCategory> _subCategories = new();
    public IReadOnlyCollection<BlogCategory> SubCategories => _subCategories.AsReadOnly();
    
    private readonly List<BlogPost> _posts = new();
    public IReadOnlyCollection<BlogPost> Posts => _posts.AsReadOnly();

    private BlogCategory() { }

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
        // Configuration değerleri: MaxCategoryNameLength=100, MaxCategoryDescriptionLength=1000
        Guard.AgainstLength(name, 100, nameof(name));
        if (description is not null)
            Guard.AgainstLength(description, 1000, nameof(description));

        if (parentCategoryId.HasValue && parentCategoryId.Value == Guid.Empty)
        {
            throw new DomainException("Geçersiz parent category ID.");
        }

        var finalSlug = slug is not null ? Slug.FromString(slug) : Slug.FromString(name);

        if (!string.IsNullOrEmpty(imageUrl) && !IsValidUrl(imageUrl))
        {
            throw new DomainException("Geçerli bir image URL giriniz.");
        }

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

        category.AddDomainEvent(new BlogCategoryCreatedEvent(category.Id, name, finalSlug.Value));

        return category;
    }

    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrEmpty(newName, nameof(newName));
        // Configuration değeri: MaxCategoryNameLength=100
        Guard.AgainstLength(newName, 100, nameof(newName));
        Name = newName;
        Slug = Slug.FromString(newName);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, newName, Slug.Value));
    }

    public void UpdateSlug(string newSlug)
    {
        Guard.AgainstNullOrEmpty(newSlug, nameof(newSlug));
        Slug = Slug.FromString(newSlug);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateDescription(string? newDescription)
    {
        // Configuration değeri: MaxCategoryDescriptionLength=1000
        if (newDescription is not null)
            Guard.AgainstLength(newDescription, 1000, nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateParentCategory(Guid? parentCategoryId)
    {
        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
        {
            throw new DomainException("Kategori kendisinin alt kategorisi olamaz.");
        }
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateImageUrl(string? newImageUrl)
    {
        if (!string.IsNullOrEmpty(newImageUrl) && !IsValidUrl(newImageUrl))
        {
            throw new DomainException("Geçerli bir image URL giriniz.");
        }
        
        ImageUrl = newImageUrl;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));
        DisplayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryActivatedEvent(Id, Name, Slug.Value));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryDeactivatedEvent(Id, Name, Slug.Value));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryDeletedEvent(Id, Name));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCategoryRestoredEvent(Id, Name, Slug.Value));
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

