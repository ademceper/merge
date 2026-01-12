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
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı (ZORUNLU)
    public Slug Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public int DisplayOrder { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;
    public string? IconUrl { get; private set; }

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public KnowledgeBaseCategory? ParentCategory { get; private set; }
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<KnowledgeBaseCategory> _subCategories = new();
    public IReadOnlyCollection<KnowledgeBaseCategory> SubCategories => _subCategories.AsReadOnly();
    
    private readonly List<KnowledgeBaseArticle> _articles = new();
    public IReadOnlyCollection<KnowledgeBaseArticle> Articles => _articles.AsReadOnly();

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private KnowledgeBaseCategory() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        var slugValueObject = Slug.FromString(slug);
        if (description != null)
            Guard.AgainstLength(description, 1000, nameof(description));
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));

        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
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

        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryCreatedEvent
        category.AddDomainEvent(new KnowledgeBaseCategoryCreatedEvent(
            category.Id,
            category.Name,
            category.Slug.Value,
            parentCategoryId));

        return category;
    }

    // ✅ BOLUM 1.1: Domain Method - Update name and slug
    public void UpdateName(string name, string slug)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        Guard.AgainstLength(name, 100, nameof(name));
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        var slugValueObject = Slug.FromString(slug);

        Name = name;
        Slug = slugValueObject;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, name, slugValueObject.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update name only (slug auto-generated from name)
    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 100, nameof(name));
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        Name = name;
        Slug = Slug.FromString(name);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update slug (manual slug update)
    public void UpdateSlug(string slug)
    {
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        Slug = Slug.FromString(slug);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update description
    public void UpdateDescription(string? description)
    {
        if (description != null)
            Guard.AgainstLength(description, 1000, nameof(description));

        Description = description;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryActivatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryActivatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryDeactivatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryDeactivatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Set active state (convenience method)
    public void SetActive(bool isActive)
    {
        if (isActive)
            Activate();
        else
            Deactivate();
    }

    // ✅ BOLUM 1.1: Domain Method - Update display order
    public void UpdateDisplayOrder(int displayOrder)
    {
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update parent category
    public void UpdateParentCategory(Guid? parentCategoryId)
    {
        // Prevent circular reference (a category cannot be its own parent)
        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
            throw new DomainException("Kategori kendi parent'ı olamaz");

        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update icon URL
    public void UpdateIconUrl(string? iconUrl)
    {
        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!string.IsNullOrEmpty(iconUrl) && !IsValidUrl(iconUrl))
        {
            throw new DomainException("Geçerli bir icon URL giriniz.");
        }
        
        IconUrl = iconUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryUpdatedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
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

        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryDeletedEvent
        AddDomainEvent(new KnowledgeBaseCategoryDeletedEvent(Id, Name, Slug.Value, ParentCategoryId));
    }

    // ✅ BOLUM 1.1: Domain Method - Restore deleted category
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseCategoryRestoredEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseCategoryRestoredEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.3: URL Validation Helper Method
    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

