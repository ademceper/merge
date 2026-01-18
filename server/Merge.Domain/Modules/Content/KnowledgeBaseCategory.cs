using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// KnowledgeBaseCategory Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class KnowledgeBaseCategory : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public int DisplayOrder { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;
    public string? IconUrl { get; private set; }

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public KnowledgeBaseCategory? ParentCategory { get; private set; }
    
    private readonly List<KnowledgeBaseCategory> _subCategories = new();
    public IReadOnlyCollection<KnowledgeBaseCategory> SubCategories => _subCategories.AsReadOnly();
    
    private readonly List<KnowledgeBaseArticle> _articles = new();
    public IReadOnlyCollection<KnowledgeBaseArticle> Articles => _articles.AsReadOnly();

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private KnowledgeBaseCategory() { }

    public static KnowledgeBaseCategory Create(
        string name,
        string slug,
        string? description = null,
        Guid? parentCategoryId = null,
        int displayOrder = 0,
        bool isActive = true,
        string? iconUrl = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        Guard.AgainstLength(name, 100, nameof(name));
        var slugValueObject = Slug.FromString(slug);
        if (description != null)
            Guard.AgainstLength(description, 1000, nameof(description));
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));

        if (!string.IsNullOrEmpty(iconUrl) && !IsValidUrl(iconUrl))
        {
            throw new DomainException("Geçerli bir icon URL giriniz.");
        }

        var category = new KnowledgeBaseCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slugValueObject,
            Description = description,
            ParentCategoryId = parentCategoryId,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            IconUrl = iconUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        category.AddDomainEvent(new KnowledgeBaseCategoryCreatedEvent(
            category.Id,
            category.Name,
            category.Slug.Value,
            parentCategoryId));

        return category;
    }

    public void UpdateName(string name, string slug)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        Guard.AgainstLength(name, 100, nameof(name));
        var slugValueObject = Slug.FromString(slug);

        Name = name;
        Slug = slugValueObject;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, name, slugValueObject.Value));
    }

    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 100, nameof(name));
        Name = name;
        Slug = Slug.FromString(name);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, name, Slug.Value));
    }

    public void UpdateSlug(string slug)
    {
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        Slug = Slug.FromString(slug);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateDescription(string? description)
    {
        if (description != null)
            Guard.AgainstLength(description, 1000, nameof(description));

        Description = description;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryActivatedEvent(Id, Name, Slug.Value));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryDeactivatedEvent(Id, Name, Slug.Value));
    }

    public void SetActive(bool isActive)
    {
        if (isActive)
            Activate();
        else
            Deactivate();
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateParentCategory(Guid? parentCategoryId)
    {
        // Prevent circular reference (a category cannot be its own parent)
        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
            throw new DomainException("Kategori kendi parent'ı olamaz");

        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateIconUrl(string? iconUrl)
    {
        if (!string.IsNullOrEmpty(iconUrl) && !IsValidUrl(iconUrl))
        {
            throw new DomainException("Geçerli bir icon URL giriniz.");
        }
        
        IconUrl = iconUrl;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        // Check if category has active articles
        if (_articles.Any(a => !a.IsDeleted))
            throw new DomainException("Aktif makaleleri olan kategori silinemez");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KnowledgeBaseCategoryDeletedEvent(Id, Name, Slug.Value, ParentCategoryId));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KnowledgeBaseCategoryRestoredEvent(Id, Name, Slug.Value));
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

